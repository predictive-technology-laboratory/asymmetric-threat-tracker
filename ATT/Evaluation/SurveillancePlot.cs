#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LAIR.Collections.Generic;
using System.IO;
using System.Diagnostics;
using LAIR.ResourceAPIs.R;
using System.Drawing.Imaging;
using LAIR.Extensions;
using PTL.ATT.Incidents;
using PTL.ATT.Models;
using PostGIS = LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.Evaluation
{
    [Serializable]
    public class SurveillancePlot : Plot
    {
        #region static members
        public static Dictionary<long, Dictionary<string, int>> GetSliceLocationTrueCount(IEnumerable<Incident> incidents, Prediction prediction)
        {
            Dictionary<long, Dictionary<string, int>> sliceLocationTrueCount = new Dictionary<long, Dictionary<string, int>>();

            DiscreteChoiceModel model = prediction.Model;
            long sliceTicks = -1;
            if (model is TimeSliceDCM)
                sliceTicks = (model as TimeSliceDCM).TimeSliceTicks;

            foreach (Incident incident in incidents)
            {
                long slice = 1;
                if (sliceTicks > 0)
                    slice = incident.Time.Ticks / sliceTicks;

                int row = (int)((incident.Location.Y - prediction.PredictionArea.BoundingBox.MinY) / prediction.PointSpacing);
                int col = (int)((incident.Location.X - prediction.PredictionArea.BoundingBox.MinX) / prediction.PointSpacing);
                string location = row + "-" + col;

                sliceLocationTrueCount.EnsureContainsKey(slice, typeof(Dictionary<string, int>));
                sliceLocationTrueCount[slice].EnsureContainsKey(location, typeof(int));
                sliceLocationTrueCount[slice][location]++;
            }

            return sliceLocationTrueCount;
        }

        public static Dictionary<string, int> GetOverallLocationTrueCount(IEnumerable<Incident> incidents, Prediction prediction)
        {
            Dictionary<string, int> overallLocationTrueCount = new Dictionary<string, int>();
            Dictionary<long, Dictionary<string, int>> sliceLocationTrueCount = GetSliceLocationTrueCount(incidents, prediction);
            foreach (long slice in sliceLocationTrueCount.Keys)
                foreach (string location in sliceLocationTrueCount[slice].Keys)
                    overallLocationTrueCount.Add(slice + "-" + location, sliceLocationTrueCount[slice][location]);

            return overallLocationTrueCount;
        }

        public static Dictionary<long, Dictionary<string, List<double>>> GetSliceLocationThreats(Prediction prediction)
        {
            Dictionary<long, Dictionary<string, List<double>>> sliceLocationThreats = new Dictionary<long, Dictionary<string, List<double>>>();

            DiscreteChoiceModel model = prediction.Model;
            long sliceTicks = -1;
            if (model is TimeSliceDCM)
                sliceTicks = (model as TimeSliceDCM).TimeSliceTicks;

            Dictionary<int, Point> idPoint = new Dictionary<int, Point>();
            foreach (Point point in prediction.Points)
                idPoint.Add(point.Id, point);

            foreach (PointPrediction pointPrediction in prediction.PointPredictions)
            {
                long slice = 1;
                if (sliceTicks > 0)
                    slice = pointPrediction.Time.Ticks / sliceTicks;

                PostGIS.Point point = idPoint[pointPrediction.PointId].Location;
                int row = (int)((point.Y - prediction.PredictionArea.BoundingBox.MinY) / prediction.PointSpacing);
                int col = (int)((point.X - prediction.PredictionArea.BoundingBox.MinX) / prediction.PointSpacing);
                string location = row + "-" + col;

                sliceLocationThreats.EnsureContainsKey(slice, typeof(Dictionary<string, List<double>>));
                sliceLocationThreats[slice].EnsureContainsKey(location, typeof(List<double>));
                sliceLocationThreats[slice][location].Add(pointPrediction.TotalThreat);
            }

            return sliceLocationThreats;
        }

        public static Dictionary<string, List<double>> GetOverallLocationThreats(Prediction prediction)
        {
            Dictionary<string, List<double>> overallLocationThreats = new Dictionary<string, List<double>>();
            Dictionary<long, Dictionary<string, List<double>>> sliceLocationThreats = GetSliceLocationThreats(prediction);
            foreach (long slice in sliceLocationThreats.Keys)
                foreach (string location in sliceLocationThreats[slice].Keys)
                    overallLocationThreats.Add(slice + "-" + location, sliceLocationThreats[slice][location]);

            return overallLocationThreats;
        }

        public static List<PointF> GetOptimalSurveillancePlotPoints(Dictionary<string, int> locationTrueCount, Dictionary<string, List<double>> locationThreats, bool percentages, bool addStartEndPoints)
        {
            // add optimal curve, created by surveilling the highest true crime areas first
            Dictionary<string, List<double>> optimalLocationThreats = new Dictionary<string, List<double>>();
            foreach (string location in locationTrueCount.SortKeysByValues())
                if (locationTrueCount[location] > 0)
                    optimalLocationThreats.Add(location, new List<double>(new double[] { optimalLocationThreats.Count + 1 }));

            // then surveil the the non-crime areas. the locationThreats variable contains a location for every region in the prediction area, so check those locations against what is already in optimalLocationThreats
            foreach (string location in locationThreats.Keys)
                if (!optimalLocationThreats.ContainsKey(location))
                    optimalLocationThreats.Add(location, new List<double>(new double[] { 0 }));

            return GetSurveillancePlotPoints(locationTrueCount, optimalLocationThreats, percentages, addStartEndPoints);
        }

        public static List<PointF> GetSurveillancePlotPoints(Dictionary<string, int> locationTrueCount, Dictionary<string, List<double>> locationThreats, bool percentages, bool addStartEndPoints)
        {
            Dictionary<string, float> locationAvgThreat = new Dictionary<string, float>(locationThreats.Count);
            foreach (string location in locationThreats.Keys)
                locationAvgThreat.Add(location, (float)locationThreats[location].Average());

            List<string> sortedLocations = locationAvgThreat.Keys.ToList();
            sortedLocations.Sort(new Comparison<string>((l1, l2) => locationAvgThreat[l2].CompareTo(locationAvgThreat[l1])));

            /* since the above sort is not stable for equal elements, it's possible to get varying results from run to run. to address this
             * make sure regions of equality within the list are always sorted in the same random way. do this by first sorting equal regions
             * by location string and then shuffling the region using a pre-seeded sequence.*/
            Random r = new Random(29729384);
            for (int start = 0; start < sortedLocations.Count - 1; )
            {
                // find region of equality
                int end = start;
                while (end < sortedLocations.Count - 1 && Math.Abs(locationAvgThreat[sortedLocations[end + 1]] - locationAvgThreat[sortedLocations[start]]) < 0.00000000000000001) { ++end; }

                // sort by location name
                sortedLocations.Sort(start, end - start + 1, null);

                // shuffle
                sortedLocations.Randomize(start, end, r);

                // find next region
                start = end + 1;
            }

            List<PointF> plotPoints = new List<PointF>();

            if (addStartEndPoints)
                plotPoints.Add(new PointF(0, 0));

            int locationsSurveilled = 0;
            int trueIncidentsCaptured = 0;
            int totalTrueIncidents = locationTrueCount.Keys.Where(location => locationThreats.ContainsKey(location)).Select(location => locationTrueCount[location]).Sum(); // only count those true incidents in locations where we made a prediction. some of our incident data falls outside the prediction area and the prediction shouldn't be penalized for not retrieving these incidents.
            int locationsSurveilledPerPlotPoint = (int)(sortedLocations.Count * 0.01);
            foreach (string location in sortedLocations)
            {
                ++locationsSurveilled;

                int trueCount;
                locationTrueCount.TryGetValue(location, out trueCount);
                trueIncidentsCaptured += trueCount;

                if ((locationsSurveilled % locationsSurveilledPerPlotPoint) == 0)
                    plotPoints.Add(new PointF(locationsSurveilled / (float)(percentages ? locationAvgThreat.Count : 1), trueIncidentsCaptured / (float)(percentages ? totalTrueIncidents : 1)));
            }

            if (addStartEndPoints)
                plotPoints.Add(new PointF(percentages ? 1 : sortedLocations.Count, percentages ? 1 : totalTrueIncidents));

            return plotPoints;
        }
        #endregion

        private Dictionary<string, float> _seriesAUC;
        private int _aucDigits;

        public Dictionary<string, float> SeriesAUC
        {
            get { return _seriesAUC; }
        }

        public SurveillancePlot(string title, Dictionary<string, List<PointF>> seriesPoints, int height, int width, Format format, int aucDigits)
            : base(title, seriesPoints, height, width, format)
        {
            _aucDigits = aucDigits;

            Render(height, width, true, false, false, false);
        }

        public SurveillancePlot(string title, Dictionary<string, List<PointF>> seriesPoints, Image image, Format format, int aucDigits)
            : base(title, seriesPoints, image, format)
        {
            _aucDigits = aucDigits;
        }

        /// <summary>
        /// Renders this surveillance plot
        /// </summary>
        /// <param name="height">Height in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="includeTitle">Whether or not to include the title</param>
        /// <param name="plotSeriesDifference">Whether or not to plot the difference between two series. Requires exactly two series in the plot.</param>
        /// <param name="blackAndWhite">Whether or not to use black and white only</param>
        /// <param name="args">Additional arguments:  1) plot margins in 0,0,0,0 format (default is 5,4,4,2), 2) additional arguments to plot and lines commands (e.g., cex), 3) additional arguments to legend command (e.g., cex)</param>
        /// <returns>Path to rendered image file</returns>
        protected override string CreateImageOnDisk(int height, int width, bool includeTitle, bool plotSeriesDifference, bool blackAndWhite, params string[] args)
        {
            if (args != null && args.Length == 0)
                args = null;

            string imageOutputPath = Path.GetTempFileName();
            List<string> tmpPaths = new List<string>();

            #region difference series
            string diffSeriesPath = null;
            PointF diffPeak = PointF.Empty;
            PointF diffValley = PointF.Empty;
            if (plotSeriesDifference)
            {
                if (SeriesPoints.Count != 2)
                    throw new Exception("Can only plot series difference using two series");

                diffSeriesPath = Path.GetTempFileName();
                tmpPaths.Add(diffSeriesPath);

                List<PointF> series1 = SeriesPoints.OrderBy(kvp => kvp.Key).First().Value;
                List<PointF> series2 = SeriesPoints.OrderBy(kvp => kvp.Key).Last().Value;

                if (series1.Count != series2.Count)
                    throw new Exception("Cannot plot series difference if series have differing numbers of points");

                StreamWriter diffSeriesFile = new StreamWriter(diffSeriesPath);
                foreach (PointF diffPoint in series1.Zip(series2, (p1, p2) =>
                    {
                        if (p1.X != p2.X)
                            throw new Exception("Differing x values in series comparison");

                        return new PointF(p1.X, p2.Y - p1.Y);
                    }))
                {
                    diffSeriesFile.WriteLine(diffPoint.X + "," + diffPoint.Y);

                    if (diffPeak == PointF.Empty || diffPoint.Y > diffPeak.Y)
                        diffPeak = diffPoint;

                    if (diffValley == PointF.Empty || diffPoint.Y < diffValley.Y)
                        diffValley = diffPoint;
                }

                diffSeriesFile.Close();
            }
            #endregion

            string imageDevice;
            if (ImageFormat == Format.EPS)
                imageDevice = "postscript(file=\"" + imageOutputPath.Replace(@"\", @"\\") + "\",onefile=FALSE,horizontal=FALSE,width=" + width + ",height=" + height + ")";
            else if (ImageFormat == Format.JPEG)
                imageDevice = "jpeg(filename=\"" + imageOutputPath.Replace(@"\", @"\\") + "\",width=" + width + ",height=" + height + ")";
            else
                throw new Exception("Unrecognized image format:  " + ImageFormat);

            StringBuilder rCmd = new StringBuilder(@"
library(zoo)
" + imageDevice + @"
" + (args == null ? "" : "par(mar=c(" + args[0] + "))") + @"
legend_labels=c()
title.lines = strsplit(strwrap(""" + Title.Replace("\"", "\\\"") + @""",width=0.6*getOption(""width"")), ""\n"")
if(length(title.lines) > 3) { title.lines = title.lines[1:3] }
main.title = paste(title.lines, sep=""\n"")");

            int seriesNum = 0;
            List<string> seriesOrder = new List<string>();
            string[] seriesDiffDummy = plotSeriesDifference ? new string[] { "dummy" } : new string[] { };
            List<int> plotCharacters = SeriesPoints.Keys.OrderBy(k => k).Union(seriesDiffDummy).Select((s, i) => ((i + 1) % 25)).ToList();  // R has 25 plot characters indexed starting at 1
            List<string> plotColors = SeriesPoints.Keys.OrderBy(k => k).Union(seriesDiffDummy).Select((s, i) => blackAndWhite ? "\"black\"" : (i + 1).ToString()).ToList(); // R color numbers start at 1 and wrap
            string aucOutputPath = Path.GetTempFileName();
            tmpPaths.Add(aucOutputPath);
            float minY = diffValley == PointF.Empty ? 0f : Math.Min(0, diffValley.Y);
            foreach (string series in SeriesPoints.Keys.OrderBy(k => k))
            {
                string pointsInputPath = Path.GetTempFileName();
                StreamWriter pointFile = new StreamWriter(pointsInputPath);
                foreach (PointF point in SeriesPoints[series])
                    pointFile.WriteLine(point.X + "," + point.Y);
                pointFile.Close();
                tmpPaths.Add(pointsInputPath);

                string plotCharacterVector = "c(" + plotCharacters[seriesNum] + ",rep(NA_integer_," + (SeriesPoints[series].Count / 10) + "))";  // show 10 plot characters for series
                string plotColor = plotColors[seriesNum];

                string seriesTitle = series.Replace("\"", "\\\"");
                if (seriesTitle.Length > 20)
                    seriesTitle = seriesTitle.Substring(0, 20);

                rCmd.Append(@"
points = read.csv(""" + pointsInputPath.Replace(@"\", @"\\") + @""",header=FALSE)
x = points[,1]
y = points[,2]
" + (seriesNum == 0 ? @"plot(x,y,type=""o"",col=" + plotColor + ",pch=" + plotCharacterVector + @",xlim=c(0,1),xlab=""% area surveilled"",ylim=c(" + minY + @",1),ylab=""% incidents captured""" + (includeTitle ? ",main=main.title" : "") + (args == null ? "" : "," + args[1]) + @")
abline(0,1,lty=""dashed"")" : @"lines(x,y,type=""o"",col=" + plotColor + ",pch=" + plotCharacterVector + (args == null ? "" : "," + args[1]) + ")") + @"
idx = order(x)
auc = round(sum(diff(x[idx])*rollmean(y[idx],2)),digits=" + _aucDigits + @")
legend_labels=c(legend_labels,paste(""" + seriesTitle + @" (AUC="",auc,"")"",sep=""""))" + @"
cat(as.character(auc),file=""" + aucOutputPath.Replace(@"\", @"\\") + @""",sep=""\n"",append=TRUE)");

                seriesNum++;
                seriesOrder.Add(series);
            }

            if (plotSeriesDifference)
            {
                string plotCharacterVector = "c(" + plotCharacters.Last() + ",rep(NA_integer_," + (SeriesPoints.Values.First().Count / 10) + "))";  // show 10 plot characters for series

                rCmd.Append(@"
points = read.csv(""" + diffSeriesPath.Replace(@"\", @"\\") + @""",header=FALSE)
x = points[,1]
y = points[,2]
lines(x,y,type=""o"",col=" + plotColors.Last() + ",pch=" + plotCharacterVector + (args == null ? "" : "," + args[1]) + @")
legend_labels=c(legend_labels,expression(paste(Delta, "" peak @ (" + string.Format("{0:0.00},{1:0.00}", diffPeak.X, diffPeak.Y) + @")"")))
abline(v=" + diffPeak.X + @",lty=1)
abline(h=" + diffPeak.Y + @",lty=1)");
            }

            rCmd.Append(@"
grid()
legend(0.4,0.4,legend_labels,pch=c(" + plotCharacters.Select(c => c.ToString()).Concatenate(",") + @"),col=c(" + plotColors.Select(c => c.ToString()).Concatenate(",") + @")" + (args == null ? "" : "," + args[2]) + @",bg=""white"")
dev.off()");

            R.Execute(rCmd.ToString(), false);

            _seriesAUC = new Dictionary<string, float>();
            string[] aucOutput = File.ReadLines(aucOutputPath).ToArray();
            for (int i = 0; i < aucOutput.Length; ++i)
                _seriesAUC.Add(seriesOrder[i], aucOutput[i] == "NA" ? float.NaN : float.Parse(aucOutput[i]));

            foreach (string tmpPath in tmpPaths)
                File.Delete(tmpPath);

            return imageOutputPath;
        }
    }
}
