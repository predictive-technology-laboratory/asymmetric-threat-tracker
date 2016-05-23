#region copyright
// Copyright 2013-2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using LAIR.MachineLearning;
using LAIR.Extensions;
using System.Drawing;
using PTL.ATT.Evaluation;
using System.Threading;
using System.IO;
using System.Collections;
using PTL.ATT.Smoothers;
using System.IO.Compression;

namespace PTL.ATT.Models
{
    [Serializable]
    public class AreaSpecificFeatureBasedDCM : FeatureBasedDCM
    {
        private List<int>_areasZipCodes =new List<int>();
        public List<int> AreasZipCodes
        {
            set
            {
                _areasZipCodes.Clear();
                _areasZipCodes.AddRange(value); 
                Update();
            }
            get
            {
                return _areasZipCodes;
            }
        }
        private string _zipcodeShapeFile;
        public string ZipcodeShapeFile { set { _zipcodeShapeFile = value; Update(); } get { return _zipcodeShapeFile; } }
        public AreaSpecificFeatureBasedDCM() : base() { }
        public AreaSpecificFeatureBasedDCM(string name,
                           IEnumerable<string> incidentTypes,
                           Area trainingArea,
                           DateTime trainingStart,
                           DateTime trainingEnd,
                           IEnumerable<Smoother> smoothers,
                           int trainingPointSpacing,
                           int featureDistanceThreshold,
                           int negativePointStandoff,
                           PTL.ATT.Classifiers.Classifier classifier,
                           IEnumerable<Feature> features,
                           string ZipCodeShapeFile,
                           List<int> zipCodes)
            : base(name, incidentTypes, trainingArea, trainingStart, trainingEnd, smoothers, trainingPointSpacing, featureDistanceThreshold, negativePointStandoff, classifier, features)
        {
            AreasZipCodes = new List<int>();
            AreasZipCodes.AddRange(zipCodes);
            this.ZipcodeShapeFile = ZipCodeShapeFile;
            Update();
        }

        public override DiscreteChoiceModel Copy()
        {
            return new AreaSpecificFeatureBasedDCM(Name, IncidentTypes, TrainingArea, TrainingStart, TrainingEnd, Smoothers, TrainingPointSpacing, FeatureDistanceThreshold, NegativePointStandoff, Classifier, Features, ZipcodeShapeFile,AreasZipCodes);
        }
        protected override void Run(Prediction prediction)
        {
            Run(prediction, true, _classifier.RunFeatureSelection, true);
        }
        protected virtual void InsertPointsIntoPrediction(NpgsqlConnection connection, Prediction prediction, bool training, bool vacuum, int zipcode)
        {
            Area area = training ? prediction.Model.TrainingArea : prediction.Model.PredictionArea;

            // insert positive points
            List<Tuple<PostGIS.Point, string, DateTime>> incidentPointTuples = new List<Tuple<PostGIS.Point, string, DateTime>>();
            foreach (Incident i in Incident.Get(prediction.Model.TrainingStart, prediction.Model.TrainingEnd, area, prediction.Model.IncidentTypes.ToArray()))
                incidentPointTuples.Add(new Tuple<PostGIS.Point, string, DateTime>(new PostGIS.Point(i.Location.X, i.Location.Y, area.Shapefile.SRID), training ? i.Type : PointPrediction.NullLabel, training ? i.Time : DateTime.MinValue)); // training points are labeled and have a time associated with them

            if (training && incidentPointTuples.Count == 0)
                Console.Out.WriteLine("WARNING:  Zero positive incident points retrieved for \"" + prediction.Model.IncidentTypes.Concatenate(", ") + "\" during the training period \"" + prediction.Model.TrainingStart.ToShortDateString() + " " + prediction.Model.TrainingStart.ToShortTimeString() + " -- " + prediction.Model.TrainingEnd.ToShortDateString() + " " + prediction.Model.TrainingEnd.ToShortTimeString() + "\"");

            List<int> ids = Point.Insert(connection, incidentPointTuples, prediction, area, vacuum);  // all incidents are constrained to be in the area upon import, so we don't need to filter them before inserting
            int maxId = ids.Max();
            // insert negative points
            int negativePointSpacing = training ? _trainingPointSpacing : prediction.PredictionPointSpacing;
            List<Tuple<PostGIS.Point, string, DateTime>> nullPointTuples = new List<Tuple<PostGIS.Point, string, DateTime>>();
            double areaMinX = area.BoundingBox.MinX;
            double areaMaxX = area.BoundingBox.MaxX;
            double areaMinY = area.BoundingBox.MinY;
            double areaMaxY = area.BoundingBox.MaxY;
            for (double x = areaMinX + negativePointSpacing / 2d; x <= areaMaxX; x += negativePointSpacing) // place points in the middle of the square boxes that cover the region - we get display errors from pixel rounding if the points are exactly on the boundaries
                for (double y = areaMinY + negativePointSpacing / 2d; y <= areaMaxY; y += negativePointSpacing)
                {
                    PostGIS.Point point = new PostGIS.Point(x, y, area.Shapefile.SRID);
                    nullPointTuples.Add(new Tuple<PostGIS.Point, string, DateTime>(point, PointPrediction.NullLabel, DateTime.MinValue)); // null points are never labeled and never have time
                }

            // filter out any negative point whose bounding box does not intersect the area
            nullPointTuples = area.Intersects(nullPointTuples.Select(t => t.Item1), negativePointSpacing / 2f).Select(i => nullPointTuples[i]).ToList();

            // filter out any negative point that is too close to a positive point -- only when training since we need all null points during prediction for a continuous surface
            if (training)
                nullPointTuples = nullPointTuples.Where(nullPointTuple => !incidentPointTuples.Any(incidentPointTuple => incidentPointTuple.Item1.DistanceTo(nullPointTuple.Item1) < _negativePointStandoff)).ToList();
            Point.Insert(connection, nullPointTuples, prediction, area, vacuum);
            Point.FilterByZipCode(connection,ZipcodeShapeFile, prediction, zipcode, vacuum, maxId);
        }
        private int Run(Prediction prediction, bool train, bool runFeatureSelection, bool predict)
        {
            if (Features.Count == 0)
                throw new Exception("Must select one or more features.");

            // all features must reference a shapefile that is valid for the prediction area's SRID -- might not be the case because we allow remapping
            Set<int> shapefilesInPredictionSRID = new Set<int>(Shapefile.GetForSRID(prediction.PredictionArea.Shapefile.SRID).Select(s => s.Id).ToArray());
            string badFeatures = Features.Where(f => (f.EnumValue.Equals(FeatureType.MinimumDistanceToGeometry) || f.EnumValue.Equals(FeatureType.GeometryDensity)) &&
                                                     !shapefilesInPredictionSRID.Contains(int.Parse(f.PredictionResourceId))).Select(f => f.ToString()).Concatenate(",");
            if (badFeatures.Length > 0)
                throw new Exception("Features \"" + badFeatures + "\" are not valid for the prediction area (" + prediction.PredictionArea.Name + "). These features must be remapped for prediction (or perhaps they were remapped incorrectly).");

            if (prediction.PredictionArea.Id != TrainingArea.Id && Features.Count(f => f.EnumValue.Equals(AreaSpecificFeatureBasedDCM.FeatureType.GeometryAttribute)) > 0)
                throw new Exception("Cannot use geometry attributes in feature-remapped predictions.");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);
            string pointTable = "point_" + prediction.Id;
            string point_predictionTable = "point_prediction_" + prediction.Id;
            #region no pooled
            if (_classifier is Classifiers.LibLinear) // no pooled
            {


                try
                {
                    AreasZipCodes.ForEach(zipCode =>
                    {
                        Console.Out.WriteLine("Creating training grids for area " + zipCode);
                        Point.CreateTable(prediction, prediction.Model.TrainingArea.Shapefile.SRID);
                        InsertPointsIntoPrediction(cmd.Connection, prediction, true, true, zipCode);
                        _classifier.Initialize();
                        #region training
                        if (train)
                        {


                            #region feature selection
                            if (runFeatureSelection)
                            {
                                Console.Out.WriteLine("Running feature selection");

                                foreach (FeatureVectorList vectors in ExtractFeatureVectors(prediction, true, TrainingStart, TrainingEnd))
                                    _classifier.Consume(vectors);

                                _classifier.Train();

                                SelectFeatures(prediction, false);
                            }
                            #endregion

                            foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, true, TrainingStart, TrainingEnd))
                                _classifier.Consume(featureVectors);

                            Console.Out.WriteLine("Training model");

                            _classifier.Train();

                            Point.DeleteTable(prediction);
                            prediction.ReleaseAllLazyLoadedData();
                        }
                        #endregion

                        #region prediction
                        if (predict)
                        {

                            Console.Out.WriteLine("Creating prediction grid for area " + zipCode);

                            Point.CreateTable(prediction, prediction.PredictionArea.Shapefile.SRID);
                            InsertPointsIntoPrediction(cmd.Connection, prediction, false, true, zipCode);

                            PointPrediction.CreateTable(prediction);
                            using (FileStream pointPredictionLogFile = new FileStream(prediction.PointPredictionLogPath, FileMode.Create, FileAccess.Write))
                            using (GZipStream pointPredictionLogGzip = new GZipStream(pointPredictionLogFile, CompressionMode.Compress))
                            using (StreamWriter pointPredictionLog = new StreamWriter(pointPredictionLogGzip))
                            {
                                foreach (FeatureVectorList featureVectors in ExtractFeatureVectors(prediction, false, prediction.PredictionStartTime, prediction.PredictionEndTime))
                                {
                                    Console.Out.WriteLine("Making predictions");

                                    _classifier.Classify(featureVectors);

                                    #region log feature values
                                    foreach (FeatureVector vector in featureVectors.OrderBy(v => (v.DerivedFrom as Point).Id))  // sort by point ID so prediction log is sorted
                                    {
                                        Point p = vector.DerivedFrom as Point;
                                        if (p == null)
                                            throw new NullReferenceException("Expected Point in vector.DerivedFrom");

                                        pointPredictionLog.Write(GetPointIdForLog(p.Id, p.Time) + " <p><ls>");
                                        foreach (string label in vector.DerivedFrom.PredictionConfidenceScores.SortKeysByValues(true))
                                            if (label == PointPrediction.NullLabel || prediction.Model.IncidentTypes.Contains(label))
                                                pointPredictionLog.Write("<l c=\"" + vector.DerivedFrom.PredictionConfidenceScores[label] + "\"><![CDATA[" + label + "]]></l>");
                                            else
                                                throw new Exception("Invalid prediction label on point:  " + label);

                                        pointPredictionLog.Write("</ls><fvs>");
                                        foreach (LAIR.MachineLearning.Feature feature in vector)
                                        {
                                            object value;
                                            if (feature is NumericFeature)
                                                value = Convert.ToSingle(vector[feature]);
                                            else
                                                value = vector[feature].ToString();

                                            pointPredictionLog.Write("<fv id=\"" + feature.Name + "\">" + value + "</fv>");
                                        }

                                        pointPredictionLog.WriteLine("</fvs></p>");
                                    }
                                    #endregion

                                    PointPrediction.Insert(GetPointPredictionValues(featureVectors), prediction, false);
                                }

                                pointPredictionLog.Close();


                                PointPrediction.VacuumTable(prediction);
                                Smooth(prediction);
                            }
                        }
                        #endregion
                        #region clean up and prepare for next prediction
                        string newpointTable = (pointTable + "_" + zipCode);

                        cmd.CommandText = "CREATE TABLE " + newpointTable + " as SELECT * FROM " + pointTable;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "VACUUM ANALYZE " + newpointTable;
                        cmd.ExecuteNonQuery();


                        string newpoint_predictionTable = (point_predictionTable + "_" + zipCode);
                        cmd.CommandText = "CREATE TABLE " + newpoint_predictionTable + " as SELECT * FROM " + point_predictionTable;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "  VACUUM ANALYZE " + newpoint_predictionTable;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "DROP TABLE " + point_predictionTable + ";  DROP TABLE  " + pointTable;
                        cmd.ExecuteNonQuery();

                        #endregion
                    });
                    Console.Out.WriteLine("Aggregate results from all areas");
                    #region aggregate results from all zipcodes
                    Point.CreateTable(prediction, prediction.PredictionArea.Shapefile.SRID);
                    PointPrediction.CreateTable(prediction);

                    AreasZipCodes.ForEach(zipCode =>
                  {

                      string newpointTable = (pointTable + "_" + zipCode);
                      string newpoint_predictionTable = (point_predictionTable + "_" + zipCode);

                      cmd.CommandText = " INSERT INTO " + pointTable + " SELECT * FROM " + newpointTable + " WHERE " + newpointTable + ".id not in (select id from " + pointTable + ")";
                      cmd.ExecuteNonQuery();
                      cmd.CommandText = " INSERT INTO " + point_predictionTable + " (labels, point_id, threat_scores, \"time\", total_threat) SELECT labels, point_id, threat_scores, \"time\", total_threat FROM " + newpoint_predictionTable;
                      cmd.ExecuteNonQuery();

                      cmd.CommandText = " DROP TABLE " + newpointTable;
                      cmd.ExecuteNonQuery();
                      cmd.CommandText = " DROP TABLE " + newpoint_predictionTable;
                      cmd.ExecuteNonQuery();
                  });
                    cmd.CommandText = "  VACUUM ANALYZE " + pointTable;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "  VACUUM ANALYZE " + point_predictionTable;
                    cmd.ExecuteNonQuery();
                    #endregion





                    return prediction.Id;

                }
                finally
                {
                    DB.Connection.Return(cmd.Connection);
                }
            }
            #endregion

            else
                return prediction.Id;
        }
        public override string ToString()
        {
            return "Area Specific Feature Based DCM:  " + Name;
        }
        public override string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            IFeatureExtractor externalFeatureExtractor = InitializeExternalFeatureExtractor(typeof(TimeSliceDCM));
            string zipCodesList = "";
            AreasZipCodes.ForEach(zipcode => zipCodesList += " " + zipcode);
            return base.GetDetails(indentLevel) + Environment.NewLine +
                   indent + "ZipCodes:  " + zipCodesList;

        }
    }
     
}
