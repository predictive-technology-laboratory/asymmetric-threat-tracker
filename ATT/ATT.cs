using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using Npgsql;
using System.Drawing;

namespace PTL.ATT
{
    /// <summary>
    /// Asymmetric threat tracker
    /// </summary>
    public class ATT
    {
        //[WebMethod(Description = "The method to generate feature table in SQL Server; calculating distance between grids and features or raster features of each grid. "+
        //"<br/>Input:gridTableName:grid table name in PostGIS database; featureValueTableName: the table to store values in SQL Server; featureTableName:the table storing feature names in SQL Server;featureType: type of feature ('d'distance or 'r'raster)"+
        //    "<br/>Output:a feature table storing feature values in SQL Server")]
        //protected string featureGene_sub(string gridTableName, string featureValueTableName, string featureTableName, string featureType, int featurePriority)
        //{

        //    if (featureValueTableName == "") featureValueTableName = ConfigurationManager.AppSettings["defaultFeatureValueTableName"];
        //    string returnmessage = "";
        //    string ftableID, gtableID;
        //    string gisUTMSrid = ConfigurationManager.AppSettings["gisUTMSrid"];
        //    string gisDefaultSrid = ConfigurationManager.AppSettings["gisDefaultSrid"];
        //    string featuretable = ConfigurationManager.AppSettings["featureTableInfoTable"];
        //    string gridtable = ConfigurationManager.AppSettings["gridTableInfoTable"];
        //    ftableID = "0"; gtableID = "0";


        //    #region database information
        //    NpgsqlConnection connPostgreSQL = new NpgsqlConnection(WebConfigurationManager.ConnectionStrings["ATTGISDB"].ConnectionString);

        //    SqlConnection connSQLServer = new SqlConnection(WebConfigurationManager.ConnectionStrings["ATTSQLDB"].ConnectionString);
        //    SqlCommand sqlCommand = new SqlCommand();
        //    sqlCommand.Connection = connSQLServer;
        //    SqlDataReader reader;
        //    #endregion


        //    int f_IDCheckSucess = 1;
        //    #region Check IDs of gridTableName and featureTableName in SQL Server
        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + featuretable + " WHERE ftablename='" + featureTableName + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            ftableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnmessage = returnmessage + "<br/>Error in reading feature table ID from " + featuretable + ".";
        //        f_IDCheckSucess = 0;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + gridtable + " WHERE gridTableName='" + gridTableName + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            gtableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnmessage = returnmessage + "<br/>Error in reading grid table ID from " + gridtable + ".";
        //        f_IDCheckSucess = 0;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    if (ftableID == "0" || gtableID == "0")
        //    {
        //        returnmessage += "<br/>+Cannot find records of feature table or grid table";
        //        f_IDCheckSucess = 0;
        //    }




        //    #endregion

        //    int f_getFeatureNameList = 0;
        //    if (f_IDCheckSucess == 1)
        //    {
        //        #region get the list of feature Names

        //        f_getFeatureNameList = 1;
        //        //get the list of feature names: featureNames saves feature names; featureID saves feature ID;
        //        _featureNames = new List<string>();
        //        _featureID = new List<string>();
        //        sqlCommand.CommandText = "SELECT ID,FeatureName FROM " + featureTableName + " WHERE FeatureType='" + featureType + "' AND priority <= '" + featurePriority.ToString() + "'";

        //        try
        //        {
        //            connSQLServer.Open();
        //            reader = sqlCommand.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                _featureID.Add(reader["ID"].ToString());
        //                _featureNames.Add(reader["FeatureName"].ToString());

        //            }
        //        }
        //        catch (Exception)
        //        {
        //            returnmessage += "Error in Reading Feature Table";
        //            f_getFeatureNameList = 0;
        //        }
        //        finally
        //        {
        //            connSQLServer.Close();
        //        }

        //        #endregion
        //    }

        //    if (f_getFeatureNameList == 1)
        //    {
        //        if (featureType == "d")
        //        {
        //            #region Distance Feature; Query in PostGIS and Insert into SQL Server
        //            bool f_fistTime = true;
        //            bool f_sqlErr = false;
        //            int gid_start = 0;
        //            int fid_err = -1;
        //            int gid = 0;
        //            int err_id_tmp = -1;
        //            string err_record = "";

        //            //there might be some features cannot be computed; we need to skip those features
        //            //the hard task is how to skip those features: if encounter an error, record the feature ID and grid ID, skip it and proceed to the next feature
        //            while ((f_fistTime) || (f_sqlErr))
        //            {
        //                f_fistTime = false;
        //                try
        //                {
        //                    connPostgreSQL.Open();
        //                    string pgsqlCommandString = "SELECT id,AsText(Centroid(the_geom)) FROM " + gridTableName + " WHERE id>=" + gid_start.ToString() + " ORDER BY id ASC" + ";";
        //                    NpgsqlCommand pgsqlCommand = new NpgsqlCommand(pgsqlCommandString, connPostgreSQL);
        //                    NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();



        //                    while (pgsqlDataReader.Read())
        //                    {

        //                        string theGeomString = "";
        //                        gid = pgsqlDataReader.GetInt32(0);
        //                        theGeomString = pgsqlDataReader.GetString(1);


        //                        for (int i = 0; i < _featureNames.Count; i++)
        //                        {
        //                            err_id_tmp = i;
        //                            double featureValues = 0;
        //                            if ((gid == gid_start) && (fid_err == i))
        //                            {
        //                                featureValues = -1;
        //                            }
        //                            else
        //                            {
        //                                string pgsqlDistanceString = "SELECT Distance(transform(the_geom," + gisUTMSrid + "),transform(ST_GeometryFromText('" + theGeomString + "'," + gisDefaultSrid + ")," + gisUTMSrid + ")) as dist ";
        //                                pgsqlDistanceString += "FROM " + _featureNames[i].ToString() + " ORDER BY dist ASC LIMIT 1;";
        //                                NpgsqlCommand pgsqlCommandDistance = new NpgsqlCommand(pgsqlDistanceString, connPostgreSQL);
        //                                NpgsqlDataReader pgsqlDistanceReader = pgsqlCommandDistance.ExecuteReader();


        //                                while (pgsqlDistanceReader.Read())
        //                                {
        //                                    featureValues = Convert.ToDouble(pgsqlDistanceReader.GetDouble(0));

        //                                }

        //                            }
        //                            string insetSQL = "INSERT INTO " + featureValueTableName + " (FID,GID,FV,FType,ftableID,gtableID) VALUES (" + _featureID[i].ToString() + "," + gid.ToString() + "," + featureValues.ToString() + ",'" + featureType + "'," + ftableID + "," + gtableID + ")";
        //                            SqlCommand sqlCommandInsert = new SqlCommand(insetSQL, connSQLServer);
        //                            err_record = insetSQL;
        //                            connSQLServer.Open();
        //                            sqlCommandInsert.ExecuteNonQuery();
        //                            connSQLServer.Close();

        //                        }



        //                    }

        //                    f_sqlErr = false;



        //                }
        //                catch (Exception)
        //                {
        //                    f_sqlErr = true;
        //                    fid_err = err_id_tmp;
        //                    gid_start = gid;
        //                    returnmessage += "Error:fid:" + fid_err.ToString() + " and gridID:" + gid_start.ToString() + " ";
        //                    returnmessage += err_record;
        //                }
        //                finally
        //                {
        //                    connPostgreSQL.Close();
        //                }


        //            }


        //            #endregion
        //        }

        //        //compute raster features: average of raster values within the grid
        //        if (featureType == "r")
        //        {


        //            #region get grid information xstep, ystep
        //            double xstep, ystep;
        //            xstep = 0; ystep = 0;
        //            sqlCommand.CommandText = "SELECT xstep,ystep FROM " + gridtable + " WHERE gridTableName='" + gridTableName + "'";


        //            try
        //            {
        //                connSQLServer.Open();
        //                reader = sqlCommand.ExecuteReader();
        //                while (reader.Read())
        //                {
        //                    xstep = Convert.ToDouble(reader["xstep"].ToString());
        //                    ystep = Convert.ToDouble(reader["ystep"].ToString());

        //                }
        //            }
        //            catch (Exception)
        //            {
        //                returnmessage += "Error in Reading xstep, ystep";
        //            }
        //            finally
        //            {
        //                connSQLServer.Close();
        //            }


        //            #endregion

        //            #region Raster Feature; Query in PostGIS and Insert into SQL Server;
        //            int gid = 0;
        //            string featureNamesErro = "";

        //            try
        //            {
        //                connPostgreSQL.Open();
        //                string pgsqlCommandString = "SELECT id,x(Centroid(the_geom)),y(Centroid(the_geom)) FROM " + gridTableName + ";";
        //                NpgsqlCommand pgsqlCommand = new NpgsqlCommand(pgsqlCommandString, connPostgreSQL);
        //                NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //                while (pgsqlDataReader.Read())
        //                {
        //                    gid = pgsqlDataReader.GetInt32(0);

        //                    double x = pgsqlDataReader.GetDouble(1);
        //                    double y = pgsqlDataReader.GetDouble(2);
        //                    string xmin = (x - xstep / (double)2).ToString();
        //                    string ymin = (y - ystep / (double)2).ToString();
        //                    string xmax = (x + xstep / (double)2).ToString();
        //                    string ymax = (y + ystep / (double)2).ToString();


        //                    for (int i = 0; i < _featureNames.Count; i++)
        //                    {
        //                        featureNamesErro = _featureNames[i].ToString();
        //                        string pgsqlRasterString = "SELECT avg(grid_code) ";
        //                        pgsqlRasterString += "FROM " + _featureNames[i].ToString();
        //                        pgsqlRasterString += " WHERE the_geom && SetSRID('BOX3D(" + xmin + " " + ymin + "," + xmax + " " + ymax + ")'::box3d," + gisDefaultSrid + ")";
        //                        NpgsqlCommand pgsqlCommandRaster = new NpgsqlCommand(pgsqlRasterString, connPostgreSQL);
        //                        //returnmessage += pgsqlRasterString;

        //                        try
        //                        {
        //                            double featureValues = Convert.ToDouble(pgsqlCommandRaster.ExecuteScalar().ToString());


        //                            string insetSQL = "INSERT INTO " + featureValueTableName + " (FID,GID,FV,FType,ftableID,gtableID) VALUES (" + _featureID[i].ToString() + "," + gid.ToString() + "," + featureValues.ToString() + ",'" + featureType + "'," + ftableID + "," + gtableID + ")";


        //                            SqlCommand sqlCommandInsert = new SqlCommand(insetSQL, connSQLServer);
        //                            connSQLServer.Open();
        //                            sqlCommandInsert.ExecuteNonQuery();
        //                            connSQLServer.Close();
        //                        }
        //                        catch (Exception)
        //                        {
        //                            //returnmessage += "<br/>Raster feature " + featureNamesErro + " is not computed at GID:" + gid.ToString() + ";";
        //                            //break;
        //                        }
        //                        finally
        //                        { }



        //                    }


        //                }

        //            }
        //            catch (Exception)
        //            {
        //                returnmessage += "<br/>Error in Inserting Raster and the grid name is " + gid.ToString() + " " + featureNamesErro;
        //            }
        //            finally
        //            {
        //                connPostgreSQL.Close();
        //            }
        //            #endregion

        //        }

        //    }

        //    //if (returnmessage == "") returnmessage = "Table is created successfully!";
        //    return returnmessage;

        //}

        //[WebMethod(Description = "The method to generate feature values in SQL Server; calculating distance between grids and features or raster features of each grid. " +
        //"<br/>Input:gridTableName:grid table name in PostGIS database; featureValueTableName: the table to store values in SQL Server; featureTableName:the table storing feature names in SQL Server; featurePriority: the priority threshold (an integer); only features with priority values less than this value will be computed" +
        //    "<br/>Output:a feature table storing feature values in SQL Server")]
        //public string featureGene(string gridTableName, string featureTableName, string featureValueTableName, int featurePriority)
        //{
        //    string returnmessage = "";
        //    returnmessage += featureGene_sub(gridTableName, featureValueTableName, featureTableName, "d", featurePriority);
        //    returnmessage += featureGene_sub(gridTableName, featureValueTableName, featureTableName, "r", featurePriority);
        //    return returnmessage;
        //}        

        

        //[WebMethod(Description = "The method to train S-T models and save models."
        //+ "<br/>Input:gridTableName:grid table name;featureTable:feature table name associated with the incidents;incidentTable: where incidents are stored in the GIS database;"
        //+ " featureValueTableName: where the feature values are stored;incidentType: incidents type interested;inciStartDate,inciEndDate: date range of incidents in consideration, only incidents happened during this period are used to train models;"
        //+ " timeInterval: how large time interval the model should use, 'd'(day),'w'(week), 'm'(month),or 'y'(year);"
        //+ "<br/>Output:The return string tells whether the task is successful or not. ")]
        //public string trainPredModel(string gridTable, string featureTable, string incidentTable, string featureValueTableName, string incidentType, string inciStartDate, string inciEndDate, char timeInterval)
        //{
        //    #region variale definition
        //    string dataLine = "";
        //    string directoryName = ConfigurationManager.AppSettings["dataFileDirectoryName"];
        //    string dataFileName = directoryName + ConfigurationManager.AppSettings["featureSTModelDataName"];

        //    string returnstring = "";

        //    string ftableID = "0";//feature table ID
        //    string gtableID = "0";//grid table ID
        //    string itableID = "0";//incident tabele ID

        //    double samplePercentage = 0.01; //train the model based on a small sample; see the paper a) for details
        //    double offset = Math.Log(samplePercentage);//used to correct biased sampling
        //    double incidentMaxPercentage = 0.5;//should not include too many incidents for training
        //    int incidentNumber = 0;//incident number used for training models: only spatial points with incident happened
        //    int sampleSize = 0;
        //    int kMax = 13;//the largest value of temporal dummy variable k to be considered
        //    int minSampleSize = 1000;//sample size should not be too small
        //    int timePeriodMaxConsidered = 1;//time period between inciStartDate and inciEndDate;         
        //    int incidentIndexCutOff = 0;//the start index value of non-incident grids; 

        //    DateTime maxDate = Convert.ToDateTime(inciEndDate);
        //    DateTime minDate = Convert.ToDateTime(inciStartDate);

        //    string featuretableInfo = ConfigurationManager.AppSettings["featureTableInfoTable"];
        //    string gridtableInfo = ConfigurationManager.AppSettings["gridTableInfoTable"];
        //    string incidentInfoTable = ConfigurationManager.AppSettings["incidentTableInfoTable"];


        //    #endregion

        //    #region reset some variable based on time intervals
        //    //whether the time interval input is valid:
        //    if (timeInterval != 'd' && timeInterval != 'm' && timeInterval != 'y' && timeInterval != 'w') return "Please input valid timeInterval value: 'd'(day),'w'(week),'m'(month),or 'y'(year)";
        //    if (minDate > maxDate) return "Please input valid timeInterval: inciStardDate is later than inciEndDate";

        //    if (timeInterval == 'd') kMax = 31;
        //    if (timeInterval == 'w') kMax = 5;
        //    if (timeInterval == 'm') kMax = 13;
        //    if (timeInterval == 'y') kMax = 10;

        //    timePeriodMaxConsidered = Time.intervalBetweenDate(minDate, maxDate, timeInterval) + 1;

        //    #endregion

        //    #region database information

        //    SqlConnection connSQLServer = new SqlConnection(WebConfigurationManager.ConnectionStrings["ATTSQLDB"].ConnectionString);
        //    NpgsqlConnection connPostgreSQL = new NpgsqlConnection(WebConfigurationManager.ConnectionStrings["ATTGISDB"].ConnectionString);

        //    SqlCommand sqlCommand = new SqlCommand();
        //    sqlCommand.Connection = connSQLServer;
        //    SqlDataReader reader;

        //    #endregion

        //    #region Check ID of featureTable, gridTable, and incidentTable

        //    Boolean f_IDCheckSucess = true;

        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + featuretableInfo + " WHERE ftablename='" + featureTable + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            ftableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnstring = returnstring + "<br/>Error in reading ID of featuretable. The feature table may not exist.";
        //        f_IDCheckSucess = false;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + gridtableInfo + " WHERE gridTableName='" + gridTable + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            gtableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnstring = returnstring + "<br/>Error in reading ID of gridtable. The grid table may not exist.";
        //        f_IDCheckSucess = false;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + incidentInfoTable + " WHERE TableName='" + incidentTable + "'";
        //        sqlCommand.CommandText = selectTableSQLString;

        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            itableID = reader["ID"].ToString();
        //        }



        //    }
        //    catch (Exception)
        //    {
        //        returnstring = returnstring + "<br/>Error in reading ID of incidentInfoTable. The incident table may not exist.";
        //        f_IDCheckSucess = false;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    if (ftableID == "0" || gtableID == "0" || itableID == "0")
        //    {
        //        returnstring += "<br/>+Cannot find records of featuretable, gridtable or incidenttable. Feature priorities will be not be computed.";
        //        f_IDCheckSucess = false;
        //    }

        //    if (!f_IDCheckSucess) return returnstring;

        //    #endregion



        //    Random r = new Random();
        //    string modelName = "mf" + ftableID + "g" + gtableID + "i" + itableID + "t" + incidentType + r.Next(1000).ToString();

        //    if (f_IDCheckSucess)
        //    {

        //        #region compute sample size
        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string selectIncidentNumberinGISDB = "SELECT count(ID) FROM " + incidentTable + " WHERE inci_type='" + incidentType + "' AND inci_date >= '" + minDate.ToShortDateString() + "' AND inci_date <= '" + maxDate.ToShortDateString() + "';";
        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectIncidentNumberinGISDB, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //            while (pgsqlDataReader.Read())
        //            {
        //                incidentNumber = Convert.ToInt32(pgsqlDataReader.GetInt64(0));
        //            }

        //        }
        //        catch (Exception)
        //        { incidentNumber = 0; }
        //        finally { connPostgreSQL.Close(); }

        //        int totalIncidentNumber = 0;
        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string selectIncidentNumberinGISDB = "SELECT count(ID) FROM " + gridTable + ";";
        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectIncidentNumberinGISDB, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //            while (pgsqlDataReader.Read())
        //            {
        //                totalIncidentNumber = Convert.ToInt32(pgsqlDataReader.GetInt64(0));
        //            }

        //        }
        //        catch (Exception)
        //        { totalIncidentNumber = 0; }
        //        finally { connPostgreSQL.Close(); }

        //        if (totalIncidentNumber <= minSampleSize) { samplePercentage = 1; offset = 0; }
        //        sampleSize = Convert.ToInt32(totalIncidentNumber * samplePercentage);
        //        if (sampleSize * incidentMaxPercentage <= incidentNumber) incidentNumber = Convert.ToInt32(sampleSize * incidentMaxPercentage);


        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string selectMaxTimePeriodinGISDB = "SELECT max(inci_date),min(inci_date) FROM " + incidentTable + " WHERE inci_type='" + incidentType + "';";
        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectMaxTimePeriodinGISDB, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();
        //            DateTime minDateTemp = minDate;

        //            while (pgsqlDataReader.Read())
        //            {
        //                minDate = pgsqlDataReader.GetDateTime(1);
        //            }
        //            //return kMaxTemp.ToString();
        //            if (minDateTemp > minDate)
        //            {
        //                minDate = minDateTemp;
        //                timePeriodMaxConsidered = Time.intervalBetweenDate(minDate, maxDate, timeInterval) + 1;
        //            }

        //        }
        //        catch (Exception)
        //        { }
        //        finally { connPostgreSQL.Close(); }

        //        #endregion

        //        #region get a list of grid
        //        int[] gridSampleIndex = new int[sampleSize];
        //        //fill the index with grid id w/incidents on it
        //        int indexCounter = 0;
        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string selectIncidentIndexinGISDB = "SELECT grid_id FROM " + incidentTable + " WHERE inci_type='" + incidentType + "' AND inci_date >= '" + minDate.ToShortDateString() + "' AND inci_date <= '" + maxDate.ToShortDateString() + "' GROUP BY grid_id ORDER BY random() LIMIT " + incidentNumber.ToString() + ";";
        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectIncidentIndexinGISDB, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //            while (pgsqlDataReader.Read() && indexCounter < sampleSize)
        //            {
        //                gridSampleIndex[indexCounter] = pgsqlDataReader.GetInt32(0);
        //                indexCounter++;
        //            }
        //            //return selectIncidentIndexinGISDB;
        //        }
        //        catch (Exception)
        //        { return "There is an error when reading grid id with incidents"; }
        //        finally { connPostgreSQL.Close(); }


        //        incidentIndexCutOff = indexCounter;

        //        //fill the index with grid id wo/ incident on it
        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string selectIncidentIndexinGISDB = "SELECT id FROM " + gridTable + " WHERE id IN ( SELECT id FROM " + gridTable + " GROUP BY id ORDER BY random() LIMIT " + sampleSize.ToString() + ") EXCEPT SELECT grid_id FROM " + incidentTable + " WHERE inci_type='" + incidentType + "' AND inci_date >= '" + minDate.ToShortDateString() + "' AND inci_date <= '" + maxDate.ToShortDateString() + "';";
        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectIncidentIndexinGISDB, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //            while (pgsqlDataReader.Read() && indexCounter < sampleSize)
        //            {
        //                gridSampleIndex[indexCounter] = pgsqlDataReader.GetInt32(0);
        //                indexCounter++;
        //            }

        //        }
        //        catch (Exception)
        //        { return "There is an error when reading grid id without incidents."; }
        //        finally { connPostgreSQL.Close(); }

        //        #endregion

        //        #region generate feature data files for R

        //        #region get the list of feature Names---Typs is raster or distance
        //        //get the list of feature names: featureNames saves feature names; featureID saves feature ID;
        //        _featureNames = new List<string>();
        //        _featureID = new List<string>();
        //        //sqlCommand.CommandText = "SELECT ID,FID FROM " + featureTable + " WHERE FeatureType='d' or FeatureType='r'";
        //        sqlCommand.CommandText = "SELECT DISTINCT FID FROM " + featureValueTableName + " WHERE FType='d' OR FType='r' AND ftableID='" + ftableID + "' AND gtableID='" + gtableID + "'";

        //        dataLine += "gid,t,k,inci";
        //        try
        //        {
        //            connSQLServer.Open();
        //            reader = sqlCommand.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                string FID = reader["FID"].ToString();
        //                string featureNameString = "f" + FID;
        //                _featureID.Add(FID);
        //                _featureNames.Add(featureNameString);
        //                dataLine += "," + featureNameString;

        //            }
        //        }
        //        catch (Exception)
        //        {
        //            returnstring += "<br/>Error in Reading Table:" + featureTable;
        //        }
        //        finally
        //        {
        //            connSQLServer.Close();
        //        }

        //        FileStream datafileStream = new FileStream(@dataFileName, FileMode.Create);
        //        StreamWriter w_data = new StreamWriter(datafileStream);
        //        w_data.WriteLine(dataLine);


        //        #endregion

        //        for (int i = 0; i < sampleSize; i++)
        //        {
        //            string gid = gridSampleIndex[i].ToString();
        //            string featureValueString = "";
        //            #region get feature values

        //            for (int j = 0; j < _featureNames.Count; j++)
        //            {
        //                sqlCommand.CommandText = "SELECT FV FROM " + featureValueTableName + " WHERE gid='" + gid.ToString() + "' AND fid='" + _featureID[j].ToString() + "' AND ftableID='" + ftableID + "' AND gtableID='" + gtableID + "'";
        //                try
        //                {
        //                    connSQLServer.Open();
        //                    reader = sqlCommand.ExecuteReader();
        //                    if (reader.Read())
        //                    {
        //                        double fv = Convert.ToDouble(reader["fv"].ToString());
        //                        featureValueString += "," + fv.ToString();
        //                    }
        //                    else
        //                    {
        //                        featureValueString += ",";
        //                    }

        //                }
        //                catch (Exception)
        //                {
        //                    returnstring += "Error in writing files";
        //                }
        //                finally { connSQLServer.Close(); }
        //            }

        //            #endregion


        //            if (i < incidentIndexCutOff)
        //            {
        //                int[] incidentInfoInci = new int[timePeriodMaxConsidered];
        //                int[] incidentInfok = new int[timePeriodMaxConsidered];
        //                for (int j = 0; j < timePeriodMaxConsidered; j++)
        //                {
        //                    incidentInfoInci[j] = 0;
        //                    incidentInfok[j] = kMax;
        //                }

        //                try
        //                {
        //                    connPostgreSQL.Open();
        //                    string selectMaxTimePeriodinGISDB = "SELECT inci_date FROM " + incidentTable + " WHERE inci_type='" + incidentType + "' AND grid_id=" + gid + " AND inci_date >= '" + minDate.ToShortDateString() + "' AND inci_date <= '" + maxDate.ToShortDateString() + "';";
        //                    NpgsqlCommand pgsqlCommand = new NpgsqlCommand(selectMaxTimePeriodinGISDB, connPostgreSQL);
        //                    NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();


        //                    while (pgsqlDataReader.Read())
        //                    {
        //                        DateTime inciTimeTemp = Convert.ToDateTime(pgsqlDataReader.GetDateTime(0));
        //                        int month_count = Time.intervalBetweenDate(minDate, inciTimeTemp, timeInterval);
        //                        if (month_count >= 0 && month_count < timePeriodMaxConsidered) { incidentInfoInci[month_count] = 1; }
        //                    }
        //                }
        //                catch (Exception)
        //                { }
        //                finally { connPostgreSQL.Close(); }

        //                int k = kMax - 1;
        //                for (int jj = 0; jj < timePeriodMaxConsidered; jj++)
        //                {
        //                    k++;
        //                    if (k > kMax) k = kMax;
        //                    incidentInfok[jj] = k;
        //                    if (incidentInfoInci[jj] == 1) k = 0;

        //                }

        //                for (int j = 0; j < timePeriodMaxConsidered; j++)
        //                {

        //                    dataLine = gid + "," + (j + 1).ToString() + "," + incidentInfok[j].ToString() + "," + incidentInfoInci[j].ToString() + featureValueString;
        //                    w_data.WriteLine(dataLine);
        //                }
        //            }
        //            else
        //            {
        //                for (int j = 0; j < timePeriodMaxConsidered; j++)
        //                {
        //                    dataLine = gid + "," + (j + 1).ToString() + "," + kMax.ToString() + ",0" + featureValueString;
        //                    w_data.WriteLine(dataLine);
        //                }
        //            }



        //        }
        //        #endregion

        //        w_data.Close();

        //        #region train the model in R
        //        string directoryName_R = ConfigurationManager.AppSettings["dataFileDirectoryName_R"];
        //        StatConnector R = new StatConnectorClass();
        //        string setuppath = "setwd('" + directoryName_R + "')";
        //        R.Init("R");
        //        R.EvaluateNoReturn(setuppath);
        //        string rstring = "att<-read.table('" + ConfigurationManager.AppSettings["featureSTModelDataName"] + "',header=T,sep=',')";
        //        R.EvaluateNoReturn(rstring);
        //        R.EvaluateNoReturn("source('cleanFun.R')");
        //        R.EvaluateNoReturn("att <- att[,clean.na(att)]");
        //        R.EvaluateNoReturn("att$offsetvalue<-rep(" + offset.ToString() + ",nrow(att))");
        //        R.EvaluateNoReturn("att.offsetvalue<-" + offset.ToString());
        //        rstring = "att.glm<-glm(inci~(.)-k+as.factor(k)+offset(offsetvalue),data=att[-c(1:2)],family='binomial')";
        //        R.EvaluateNoReturn(rstring);
        //        //R.EvaluateNoReturn("att.step<-step(att.glm)");
        //        R.EvaluateNoReturn("model<-att.glm");
        //        R.EvaluateNoReturn("save(model,att,att.offsetvalue, file='" + modelName + ".rda')");

        //        #endregion

        //        if (returnstring == "")
        //        {
        //            string insertString = "INSERT INTO " + ConfigurationManager.AppSettings["modelTable"];
        //            insertString += " (mname,gtableID,ftableID,itableID,inci_type,offset,s_tmodel,inci_tstart,inci_tend) VALUES ";
        //            insertString += " ('" + modelName + "','" + gtableID + "','" + ftableID + "','" + itableID + "','" + incidentType + "','" + offset.ToString() + "','" + timeInterval.ToString() + "','" + inciStartDate + "','" + inciEndDate + "')";
        //            if (!DB.MsExecuteNonQuery(insertString))
        //                returnstring += "Error in inserting model information;" + insertString;
        //        }
        //    }

        //    if (returnstring == "")
        //        returnstring += "Model Name:" + modelName;

        //    return returnstring;
        //}

        //[WebMethod(Description = "The method to predict values of AO using new S-T model."
        //+ "<br/>Input:long_ul,long_rl,lat_ul,lat_rl: longitude and latitude of AOI; featureTableName:feature table name associated with the incidents;gridTableName:grid table name;featureValueTableName:where the feature values are stored;modelName: which model to use; timeToPredict: the time of predition"
        //  + "<br/>Output: DataSet Storing predicted values; if there is any error, return an empty dataset")]
        //public DataSet prediction(string gridTableName, string featureTableName, string incidentTable, double long_ul, double lat_ul, double long_rl, double lat_rl, string featureValueTableName, string modelName, string timeToPredict, string incidentType)
        //{

        //    #region variable definition
        //    string modelTable = ConfigurationManager.AppSettings["modelTable"];
        //    char intervalLength = ' ';

        //    DataSet ds = new DataSet();
        //    string message = "";
        //    #endregion

        //    #region database information

        //    SqlConnection connSQLServer = new SqlConnection(WebConfigurationManager.ConnectionStrings["ATTSQLDB"].ConnectionString);

        //    SqlCommand sqlCommand = new SqlCommand();
        //    sqlCommand.Connection = connSQLServer;
        //    SqlDataReader reader;

        //    #endregion

        //    #region check information about the model
        //    Boolean f_modelCorrect = true;
        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT s_tmodel FROM " + modelTable + " WHERE mname='" + modelName + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            intervalLength = Convert.ToChar(reader["s_tmodel"]);
        //        }

        //    }
        //    catch (Exception)
        //    { f_modelCorrect = false; }
        //    finally
        //    { connSQLServer.Close(); }
        //    #endregion

        //    if (intervalLength == ' ' || !f_modelCorrect) return ds;


        //    message = genePredFeatureDataSTFiles(gridTableName, featureTableName, incidentTable, long_ul, lat_ul, long_rl, lat_rl, featureValueTableName, Convert.ToDateTime(timeToPredict), incidentType, intervalLength);

        //    if (message == "")
        //        ds = predictioninR_ST(ConfigurationManager.AppSettings["predictionFeatureDataName"], modelName);
        //    return ds;
        //}


        ///* The function to generate data file for prediction: the supporting function for "public DataSet prediction"
        // * Input: gridTable,featureTable,incidentTable: grid table, feature table, incident table names; featureValueTable: where feature values are stored;
        // * 			long_ul,long_rl,lat_ul,lat_rl: box of AOI, where to predict; timeToPredict: time to make predictions; incidentType: what type of incident is to be predicted; intervalLength:how large time interval the model should use, 'd'(day),'w'(week), 'm'(month),or 'y'(year);"
        // * Output: a date file name; if there is a problem, return "".			
        // */
        //protected string genePredFeatureDataSTFiles(string gridTableName, string featureTableName, string incidentTable, double long_ul, double lat_ul, double long_rl, double lat_rl, string featureValueTableName, DateTime timeToPredict, string incidentType, char intervalLength)
        //{

        //    #region variale definition


        //    string ftableID = "0";
        //    string gtableID = "0";
        //    string itableID = "0";
        //    string gisDefaultSrid = ConfigurationManager.AppSettings["gisDefaultSrid"];

        //    string featuretableInfo = ConfigurationManager.AppSettings["featureTableInfoTable"];
        //    string gridtableInfo = ConfigurationManager.AppSettings["gridTableInfoTable"];
        //    string incidentInfoTable = ConfigurationManager.AppSettings["incidentTableInfoTable"];

        //    string returnmessage = "";
        //    string dataLine = "";
        //    string directoryName = ConfigurationManager.AppSettings["dataFileDirectoryName"];
        //    string dataFileName = directoryName + ConfigurationManager.AppSettings["predictionFeatureDataName"];
        //    #endregion

        //    #region database information

        //    NpgsqlConnection connPostgreSQL = new NpgsqlConnection(WebConfigurationManager.ConnectionStrings["ATTGISDB"].ConnectionString);
        //    SqlConnection connSQLServer = new SqlConnection(WebConfigurationManager.ConnectionStrings["ATTSQLDB"].ConnectionString);

        //    SqlCommand sqlCommand = new SqlCommand();
        //    sqlCommand.Connection = connSQLServer;
        //    SqlDataReader reader;

        //    #endregion

        //    int f_IDCheckSucess = 1;
        //    #region Check IDs of gridTableName and featureTableName in SQL Server
        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + featuretableInfo + " WHERE ftablename='" + featureTableName + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            ftableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnmessage = returnmessage + "<br/>Error in reading ID of featuretable.";
        //        f_IDCheckSucess = 0;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + gridtableInfo + " WHERE gridTableName='" + gridTableName + "'";
        //        sqlCommand.CommandText = selectTableSQLString;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            gtableID = reader["ID"].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        returnmessage = returnmessage + "<br/>Error in reading ID of gridtable.";
        //        f_IDCheckSucess = 0;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }


        //    try
        //    {
        //        connSQLServer.Open();

        //        string selectTableSQLString = "SELECT ID FROM " + incidentInfoTable + " WHERE TableName='" + incidentTable + "'";
        //        sqlCommand.CommandText = selectTableSQLString;

        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            itableID = reader["ID"].ToString();
        //        }



        //    }
        //    catch (Exception)
        //    {
        //        returnmessage = returnmessage + "<br/>Error in reading ID of incidentInfoTable. The incident table may not exist.";
        //        f_IDCheckSucess = 0;
        //    }
        //    finally
        //    {
        //        connSQLServer.Close();
        //    }

        //    if (ftableID == "0" || gtableID == "0" || itableID == "0")
        //    {
        //        returnmessage += "<br/>+Cannot find records of featuretable or gridtable";
        //        f_IDCheckSucess = 0;
        //    }

        //    #endregion

        //    if (f_IDCheckSucess == 1)
        //    {
        //        #region get the list of feature Names---Typs is raster or distance
        //        //get the list of raster feature names: featureNames saves feature names; featureID saves feature ID;
        //        _featureNames = new List<string>();
        //        _featureID = new List<string>();

        //        sqlCommand.CommandText = "SELECT DISTINCT FID FROM " + featureValueTableName + " WHERE FType='d' OR FType='r' AND ftableID='" + ftableID + "' AND gtableID='" + gtableID + "'";

        //        //sqlCommand.CommandText = "SELECT ID,FID FROM " + featureTableName + " WHERE FeatureType='r' OR FeatureType='d'";

        //        dataLine += "gid,long,lat,k";
        //        try
        //        {
        //            connSQLServer.Open();
        //            reader = sqlCommand.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                string FID = reader["FID"].ToString();
        //                string featureNameString = "f" + FID;
        //                _featureID.Add(FID);
        //                _featureNames.Add(featureNameString);
        //                dataLine += "," + featureNameString;

        //            }
        //        }
        //        catch (Exception)
        //        {
        //            returnmessage += "Error in Reading Table";
        //        }
        //        finally
        //        {
        //            connSQLServer.Close();
        //        }

        //        #endregion

        //        #region generate the data file
        //        FileStream datafileStream = new FileStream(@dataFileName, FileMode.Create);
        //        StreamWriter w_data = new StreamWriter(datafileStream);
        //        w_data.WriteLine(dataLine);

        //        int gid = 0;
        //        try
        //        {
        //            connPostgreSQL.Open();
        //            string pgsqlCommandString = "SELECT id,x(Centroid(the_geom)),y(Centroid(the_geom)) FROM " + gridTableName;
        //            pgsqlCommandString += " WHERE the_geom && SetSRID('BOX3D(" + long_ul.ToString() + " " + lat_ul.ToString() + "," + long_rl.ToString() + " " + lat_rl.ToString() + ")'::box3d," + gisDefaultSrid + ")";

        //            NpgsqlCommand pgsqlCommand = new NpgsqlCommand(pgsqlCommandString, connPostgreSQL);
        //            NpgsqlDataReader pgsqlDataReader = pgsqlCommand.ExecuteReader();

        //            while (pgsqlDataReader.Read())
        //            {

        //                gid = pgsqlDataReader.GetInt32(0);
        //                double xlong = pgsqlDataReader.GetDouble(1);
        //                double ylat = pgsqlDataReader.GetDouble(2);

        //                dataLine = gid.ToString() + "," + xlong.ToString() + "," + ylat.ToString();

        //                int kMax = 60;
        //                if (intervalLength == 'd') kMax = 31;
        //                if (intervalLength == 'w') kMax = 5;
        //                if (intervalLength == 'm') kMax = 13;
        //                if (intervalLength == 'y') kMax = 10;

        //                try
        //                {

        //                    string selectIncidentNumberinGISDB = "SELECT inci_date FROM " + incidentTable + " WHERE inci_type='" + incidentType + "' AND grid_id='" + gid + "' AND inci_date <= '" + timeToPredict.ToShortDateString() + "' ORDER BY inci_date DESC LIMIT 1;";
        //                    NpgsqlCommand pgsqlKCommand = new NpgsqlCommand(selectIncidentNumberinGISDB, connPostgreSQL);
        //                    NpgsqlDataReader pgsqlKDataReader = pgsqlKCommand.ExecuteReader();

        //                    while (pgsqlKDataReader.Read())
        //                    {
        //                        DateTime timeTemp = Convert.ToDateTime(pgsqlDataReader.GetDateTime(0));
        //                        kMax = Time.intervalBetweenDate(timeTemp, timeToPredict, intervalLength);
        //                    }

        //                }
        //                catch (Exception)
        //                { }
        //                finally { }

        //                dataLine += "," + kMax;

        //                for (int i = 0; i < _featureNames.Count; i++)
        //                {
        //                    sqlCommand.CommandText = "SELECT FV FROM " + featureValueTableName + " WHERE gid='" + gid.ToString() + "' AND fid='" + _featureID[i].ToString() + "' AND ftableID='" + ftableID + "' AND gtableID='" + gtableID + "'";
        //                    try
        //                    {
        //                        connSQLServer.Open();
        //                        reader = sqlCommand.ExecuteReader();
        //                        if (reader.Read())
        //                        {
        //                            dataLine += "," + reader["fv"].ToString();
        //                        }
        //                        else
        //                        {
        //                            dataLine += ",";
        //                        }

        //                    }
        //                    catch (Exception)
        //                    {
        //                        returnmessage += "Error in writing files";
        //                    }
        //                    finally
        //                    {
        //                        connSQLServer.Close();
        //                    }

        //                }
        //                w_data.WriteLine(dataLine);


        //            }



        //        }
        //        catch (Exception)
        //        {
        //            returnmessage += "Error in Write file ";
        //        }
        //        finally
        //        {
        //            connPostgreSQL.Close();
        //        }

        //        w_data.Flush();
        //        w_data.Close();
        //        #endregion
        //    }

        //    //if (returnmessage == "") returnmessage = "File is generated successfully!";
        //    return returnmessage;



        //}

        ///*The function to make prediction in R: the supporting function for "public DataSet prediction"
        // * Input: predictionDataFileName: data file for prediction; modelName: what model to use for prediction
        // * Output: prediction of risk in the format of dataset.
        // */

        //protected DataSet predictioninR_ST(string predictionDataFileName, string modelName)
        //{
        //    //string returnmessage = "";
        //    // initialize a new COM connection to R
        //    DataSet ds = new DataSet();

        //    #region prediction in R
        //    string directoryName_R = ConfigurationManager.AppSettings["dataFileDirectoryName_R"];
        //    StatConnector r = new StatConnectorClass();
        //    string setuppath = "setwd('" + directoryName_R + "')";
        //    r.Init("R");
        //    string loadModelString = "load('" + modelName + ".rda')";
        //    r.EvaluateNoReturn(setuppath);
        //    r.EvaluateNoReturn(loadModelString);
        //    r.EvaluateNoReturn("pred.data<-read.table('" + predictionDataFileName + "',header=T,sep=',')");

        //    r.EvaluateNoReturn("pred.id<-pred.data[,c('gid','long','lat')]");
        //    r.EvaluateNoReturn("pred.feature<-pred.data[,!(names(pred.data) %in% c('gid','long','lat'))]");
        //    r.EvaluateNoReturn("k.uni<-c(unique(att$k))");
        //    r.EvaluateNoReturn("k.sort<-sort(k.uni)");
        //    r.EvaluateNoReturn("for(i in c(1:(length(k.sort)-1))){pred.feature$k[which(k.sort[i]<=pred.feature$k & pred.feature$k<k.sort[i+1])]<-k.sort[i]}");

        //    r.EvaluateNoReturn("pred.feature$k[which(pred.feature$k>max(att$k))]<-max(att$k)");
        //    r.EvaluateNoReturn("pred.feature$k[which(pred.feature$k<min(att$k))]<-min(att$k)");
        //    r.EvaluateNoReturn("pred.feature$offsetvalue<-rep(att.offsetvalue,nrow(pred.feature))");
        //    r.EvaluateNoReturn("p<-predict(model,newdata=pred.feature,type = 'response')");
        //    r.EvaluateNoReturn("data.write<-cbind(pred.id,p);");
        //    r.EvaluateNoReturn("write.table(data.write,'" + ConfigurationManager.AppSettings["predictionDataName"] + "',sep=',',quote=FALSE)");



        //    #endregion

        //    #region Import prediction result into GIS DB, SQL Server DB

        //    string directoryName = ConfigurationManager.AppSettings["dataFileDirectoryName"];
        //    string dataFileName = directoryName + ConfigurationManager.AppSettings["predictionDataName"];
        //    StreamReader read = File.OpenText(@dataFileName);

        //    string inputString = "";
        //    inputString = read.ReadLine();
        //    inputString = read.ReadLine();

        //    ds.Tables.Add("Prediction");
        //    ds.Tables["Prediction"].Columns.Add("id", typeof(Int16));
        //    ds.Tables["Prediction"].Columns.Add("long", typeof(Double));
        //    ds.Tables["Prediction"].Columns.Add("lat", typeof(Double));
        //    ds.Tables["Prediction"].Columns.Add("p", typeof(Double));
        //    while (inputString != null)
        //    {
        //        string[] prediction_values = inputString.Split(',');

        //        DataRow dr = ds.Tables["Prediction"].NewRow();
        //        dr["id"] = Convert.ToInt16(prediction_values[0]);
        //        dr["long"] = Convert.ToDouble(prediction_values[2]);
        //        dr["lat"] = Convert.ToDouble(prediction_values[3]);
        //        dr["p"] = Convert.ToDouble(prediction_values[4]);
        //        ds.Tables["Prediction"].Rows.Add(dr);
        //        inputString = read.ReadLine();
        //    }
        //    read.Close();

        //    #endregion

        //    return ds;

        //}
    }
}