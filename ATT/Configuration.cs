using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAIR.XML;
using System.IO;
using LAIR.MachineLearning.ClassifierWrappers.LibLinear;
using LAIR.ResourceAPIs.R;
using PTL.ATT.Incidents;

namespace PTL.ATT
{
    /// <summary>
    /// Represents an ATT configuration
    /// </summary>
    public static class Configuration
    {
        #region postgres
        private static string _postgresHost;
        private static int _postgresPort;
        private static bool _postgresSSL;
        private static string _postgresDatabase;
        private static string _postgresUser;
        private static string _postgresPassword;
        private static int _postgresConnectionTimeout;
        private static int _postgresRetryLimit;
        private static int _postgresCommandTimeout;
        private static int _postgresMaxPoolSize;

        public static int PostgresMaxPoolSize
        {
            get { return Configuration._postgresMaxPoolSize; }
            set { Configuration._postgresMaxPoolSize = value; }
        }

        public static string PostgresHost
        {
            get { return _postgresHost; }
            set { _postgresHost = value; }
        }

        public static int PostgresPort
        {
            get { return _postgresPort; }
            set { _postgresPort = value; }
        }

        public static bool PostgresSSL
        {
            get { return _postgresSSL; }
            set { _postgresSSL = value; }
        }

        public static string PostgresDatabase
        {
            get { return _postgresDatabase; }
            set { _postgresDatabase = value; }
        }

        public static string PostgresUser
        {
            get { return _postgresUser; }
            set { _postgresUser = value; }
        }

        public static string PostgresPassword
        {
            get { return _postgresPassword; }
            set { _postgresPassword = value; }
        }

        public static int PostgresConnectionTimeout
        {
            get { return Configuration._postgresConnectionTimeout; }
            set { Configuration._postgresConnectionTimeout = value; }
        }

        public static int PostgresRetryLimit
        {
            get { return Configuration._postgresRetryLimit; }
            set { Configuration._postgresRetryLimit = value; }
        }

        public static int PostgresCommandTimeout
        {
            get { return Configuration._postgresCommandTimeout; }
            set { Configuration._postgresCommandTimeout = value; }
        }
        #endregion

        #region postgis
        private static string _shp2pgsqlPath;
        private static int _postgisSRID;
        private static double _areaBoundingBoxSize;

        public static string Shp2PgsqlPath
        {
            get { return _shp2pgsqlPath; }
            set { _shp2pgsqlPath = value; }
        }
        public static int PostgisSRID
        {
            get { return Configuration._postgisSRID; }
            set { Configuration._postgisSRID = value; }
        }

        public static double AreaBoundingBoxSize
        {
            get { return Configuration._areaBoundingBoxSize; }
        }
        #endregion

        #region r
        private static string _rCranMirror;

        public static string RCranMirror
        {
            get { return Configuration._rCranMirror; }
            set { Configuration._rCranMirror = value; }
        }
        #endregion

        #region classifiers
        private static Dictionary<Type, Dictionary<string, string>> _classifierTypeOptions;

        public static Dictionary<Type, Dictionary<string, string>> ClassifierTypeOptions
        {
            get { return Configuration._classifierTypeOptions; }
            set { Configuration._classifierTypeOptions = value; }
        }
        #endregion

        #region incident
        private static Importer _incidentImporter;
        private static int _incidentHourOffset;
        private static int _incidentNativeLocationSRID;

        public static Importer IncidentImporter
        {
            get { return Configuration._incidentImporter; }
            set { Configuration._incidentImporter = value; }
        }

        public static int IncidentHourOffset
        {
            get { return Configuration._incidentHourOffset; }
            set { Configuration._incidentHourOffset = value; }
        }

        public static int IncidentNativeLocationSRID
        {
            get { return Configuration._incidentNativeLocationSRID; }
            set { Configuration._incidentNativeLocationSRID = value; }
        }
        #endregion

        #region models
        private static string _modelsDirectory;
        private static Dictionary<Type, Type> _modelTypeFeatureExtractorType;
        private static Dictionary<Type, Dictionary<string, string>> _modelTypeFeatureExtractorConfigOptions;

        public static string ModelsDirectory
        {
            get
            {
                if (!Directory.Exists(Configuration._modelsDirectory))
                    throw new DirectoryNotFoundException("Models directory has not yet been set");

                return Configuration._modelsDirectory;
            }
            set
            {
                Configuration._modelsDirectory = value;
                if (!Directory.Exists(Configuration._modelsDirectory))
                    Directory.CreateDirectory(Configuration._modelsDirectory);
            }
        }
        #endregion

        #region java
        private static string _javaExePath;

        public static string JavaExePath
        {
            get { return Configuration._javaExePath; }
            set { Configuration._javaExePath = value; }
        }
        #endregion        

        #region system
        private static int _processorCount;

        public static int ProcessorCount
        {
            get { return Configuration._processorCount; }
            set { Configuration._processorCount = value; }
        }
        #endregion

        public static void Initialize(string path, bool initializeDB)
        {
            XmlParser p = new XmlParser(File.ReadAllText(path));

            XmlParser postgresP = new XmlParser(p.OuterXML("postgres"));
            _postgresHost = postgresP.ElementText("host");
            _postgresPort = int.Parse(postgresP.ElementText("port"));
            _postgresSSL = bool.Parse(postgresP.ElementText("ssl"));
            _postgresDatabase = postgresP.ElementText("database");
            _postgresUser = postgresP.ElementText("user");
            _postgresPassword = postgresP.ElementText("password");
            _postgresConnectionTimeout = int.Parse(postgresP.ElementText("connection_timeout"));
            _postgresRetryLimit = int.Parse(postgresP.ElementText("connection_retry_limit"));
            _postgresCommandTimeout = int.Parse(postgresP.ElementText("command_timeout"));
            _postgresMaxPoolSize = int.Parse(postgresP.ElementText("max_pool_size"));

            XmlParser postgisP = new XmlParser(p.OuterXML("postgis"));
            _shp2pgsqlPath = postgisP.ElementText("shp2pgsql");
            _postgisSRID = int.Parse(postgisP.ElementText("srid"));
            _areaBoundingBoxSize = double.Parse(postgisP.ElementText("area_bounding_box_size"));

            if (!File.Exists(_shp2pgsqlPath))
                throw new FileNotFoundException("Failed to locate shp2pgsql executable. Check configuration.");

            XmlParser rP = new XmlParser(p.OuterXML("r"));
            string rExePath = rP.ElementText("exe_path");
            if (rExePath == null || !File.Exists(rExePath))
                rExePath = Environment.GetEnvironmentVariable("R_EXE");

            _rCranMirror = rP.ElementText("cran_mirror");

            R.ExePath = rExePath;
            List<string> missingRPackages = R.CheckForMissingPackages(new string[] { "zoo", "ks", "earth", "geoR" });
            if (missingRPackages.Count > 0)
                R.InstallPackages(missingRPackages, _rCranMirror);

            XmlParser javaP = new XmlParser(p.OuterXML("java"));
            _javaExePath = javaP.ElementText("exe_path");
            if (_javaExePath == null || !File.Exists(_javaExePath))
                _javaExePath = Environment.GetEnvironmentVariable("JAVA_EXE");

            if (!File.Exists(_javaExePath))
                throw new FileNotFoundException("Failed to locate java.exe excutable. Check configuration.");

            _classifierTypeOptions = new Dictionary<Type, Dictionary<string, string>>();
            XmlParser classifiersP = new XmlParser(p.OuterXML("classifiers"));
            string classifierXML;
            while ((classifierXML = classifiersP.OuterXML("classifier")) != null)
            {
                XmlParser classifierP = new XmlParser(classifierXML);
                Type type = Reflection.GetType(classifierP.AttributeValue("classifier", "type"));

                Dictionary<string, string> optionValue = new Dictionary<string, string>();
                string option;
                while ((option = classifierP.MoveToElementNode(false)) != null)
                    optionValue.Add(option, classifierP.ElementText(option));

                _classifierTypeOptions.Add(type, optionValue);
            }

            XmlParser incidentsP = new XmlParser(p.OuterXML("incidents"));
            _incidentImporter = Activator.CreateInstance(Reflection.GetType(incidentsP.ElementText("importer"))) as Importer;
            _incidentHourOffset = int.Parse(incidentsP.ElementText("hour_offset"));
            _incidentNativeLocationSRID = int.Parse(incidentsP.ElementText("native_location_srid"));

            XmlParser modelingP = new XmlParser(p.OuterXML("modeling"));
            _modelsDirectory = modelingP.ElementText("model_directory");
            if (_modelsDirectory != "" && !Directory.Exists(_modelsDirectory))
                Directory.CreateDirectory(_modelsDirectory);

            _modelTypeFeatureExtractorType = new Dictionary<Type, Type>();
            _modelTypeFeatureExtractorConfigOptions = new Dictionary<Type, Dictionary<string, string>>();
            string featureExtractorsXML = modelingP.OuterXML("feature_extractors");
            if (featureExtractorsXML != null)
            {
                XmlParser featureExtractorsP = new XmlParser(featureExtractorsXML);
                string featureExtractorXML;
                while ((featureExtractorXML = featureExtractorsP.OuterXML("feature_extractor")) != null)
                {
                    XmlParser featureExtractorConfigP = new XmlParser(featureExtractorXML);

                    Dictionary<string, string> configOptions = new Dictionary<string, string>();
                    foreach (string option in featureExtractorConfigP.GetAttributeNames("feature_extractor"))
                        configOptions.Add(option, featureExtractorConfigP.AttributeValue("feature_extractor", option));

                    Type modelType = Reflection.GetType(configOptions["model_type"]);
                    Type featureExtractorType = Reflection.GetType(featureExtractorConfigP.ElementText("feature_extractor"), null);

                    _modelTypeFeatureExtractorType.Add(modelType, featureExtractorType);
                    _modelTypeFeatureExtractorConfigOptions.Add(modelType, configOptions);
                }
            }

            XmlParser systemP = new XmlParser(p.OuterXML("system"));
            _processorCount = int.Parse(systemP.ElementText("processor_count"));
            if (_processorCount == -1)
                _processorCount = Environment.ProcessorCount;

            if (_processorCount <= 0)
                throw new Exception("Invalid processor count (must be >= 1):  " + _processorCount);

            if (initializeDB)
                DB.Initialize();
        }

        public static bool TryGetFeatureExtractorType(Type modelType, out Type featureExtractorType)
        {
            return _modelTypeFeatureExtractorType.TryGetValue(modelType, out featureExtractorType);
        }

        public static Dictionary<string, string> GetFeatureExtractorConfigOptions(Type modelType)
        {
            return _modelTypeFeatureExtractorConfigOptions[modelType];
        }
    }
}
