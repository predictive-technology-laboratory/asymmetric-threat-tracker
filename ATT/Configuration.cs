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
using LAIR.XML;
using System.IO;
using LAIR.MachineLearning.ClassifierWrappers.LibLinear;
using LAIR.ResourceAPIs.R;
using PTL.ATT.Models;
using System.Reflection;
using LAIR.Extensions;

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
        private static string _pgsql2shpPath;
        private static string _postgisShapefileDirectory;

        public static string Shp2PgsqlPath
        {
            get { return _shp2pgsqlPath; }
            set { _shp2pgsqlPath = value; }
        }

        public static string Pgsql2ShpPath
        {
            get { return _pgsql2shpPath; }
            set { _pgsql2shpPath = value; }
        }

        public static string PostGisShapefileDirectory
        {
            get { return Configuration._postgisShapefileDirectory; }
            set { Configuration._postgisShapefileDirectory = value; }
        }
        #endregion

        #region r
        private static string _rCranMirror;
        private static string _rPackageInstallDirectory;

        public static string RPackageInstallDirectory
        {
            get { return Configuration._rPackageInstallDirectory; }
            set { Configuration._rPackageInstallDirectory = value; }
        }

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

        #region incidents
        private static string _incidentsImportDirectory;

        public static string IncidentsImportDirectory
        {
            get { return Configuration._incidentsImportDirectory; }
            set { Configuration._incidentsImportDirectory = value; }
        }
        #endregion

        #region events
        private static string _eventsImportDirectory;

        public static string EventsImportDirectory
        {
            get { return Configuration._eventsImportDirectory; }
            set { Configuration._eventsImportDirectory = value; }
        }
        #endregion

        #region importers
        private static string _importersLoadDirectory;

        public static string ImportersLoadDirectory
        {
            get { return Configuration._importersLoadDirectory; }
            set { Configuration._importersLoadDirectory = value; }
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

        private static string _path = null;
        private static bool _initialized = false;

        public static string LicenseText
        {
            get
            {
                return @"Copyright 2013-2014 The Rector & Visitors of the University of Virginia

Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this program except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.";
            }
        }

        private static string _externalFeatureExtractorDirectory;

        public static void Initialize(string path, bool initializeDB)
        {
            if(_initialized)
            {
                Console.Out.WriteLine("ATT configuration is already initialized");
                return;
            }

            _path = path;

            XmlParser p = new XmlParser(File.ReadAllText(_path));

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
            _pgsql2shpPath = postgisP.ElementText("pgsql2shp");
            _postgisShapefileDirectory = postgisP.ElementText("shapefile_directory");

            if (string.IsNullOrWhiteSpace(_shp2pgsqlPath) || !File.Exists(_shp2pgsqlPath))
                throw new FileNotFoundException("Failed to locate shp2pgsql executable. Check configuration.");

            if (string.IsNullOrWhiteSpace(_pgsql2shpPath) || !File.Exists(_pgsql2shpPath))
                throw new FileNotFoundException("Failed to locate shp2pgsql executable. Check configuration.");

            XmlParser rP = new XmlParser(p.OuterXML("r"));

            string rExePath = rP.ElementText("exe_path");
            if (string.IsNullOrWhiteSpace(rExePath) || !File.Exists(rExePath))
                rExePath = Environment.GetEnvironmentVariable("R_EXE");

            R.ExePath = rExePath;

            _rPackageInstallDirectory = rP.ElementText("package_install_directory");
            if (!string.IsNullOrWhiteSpace(_rPackageInstallDirectory))
            {
                if (!Directory.Exists(_rPackageInstallDirectory))
                    Directory.CreateDirectory(_rPackageInstallDirectory);

                R.AddLibPath(_rPackageInstallDirectory);
            }

            _rCranMirror = rP.ElementText("cran_mirror");

            XmlParser javaP = new XmlParser(p.OuterXML("java"));
            _javaExePath = javaP.ElementText("exe_path");
            if (string.IsNullOrWhiteSpace(_javaExePath) || !File.Exists(_javaExePath))
                _javaExePath = Environment.GetEnvironmentVariable("JAVA_EXE");

            if (string.IsNullOrWhiteSpace(_javaExePath) || !File.Exists(_javaExePath))
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
            _incidentsImportDirectory = incidentsP.ElementText("import_directory");

            XmlParser eventsP = new XmlParser(p.OuterXML("events"));
            _eventsImportDirectory = eventsP.ElementText("import_directory");

            XmlParser importersP = new XmlParser(p.OuterXML("importers"));
            _importersLoadDirectory = importersP.ElementText("load_directory");

            XmlParser modelingP = new XmlParser(p.OuterXML("modeling"));
            _modelsDirectory = modelingP.ElementText("model_directory");
            if (string.IsNullOrWhiteSpace(_modelsDirectory))
                _modelsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "att", "models");

            if (!Directory.Exists(_modelsDirectory))
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

                    // get external feature extractor type
                    string featureExtractorTypeStr = featureExtractorConfigP.ElementText("feature_extractor");
                    string[] parts = featureExtractorTypeStr.Split('@');
                    if (parts.Length > 1)
                    {
                        _externalFeatureExtractorDirectory = Path.GetDirectoryName(parts[1]);
                        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadExternalFeatureExtractorAssembly);
                    }

                    Type featureExtractorType = Reflection.GetType(featureExtractorTypeStr, null);

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

            _initialized = true;
        }

        /// <summary>
        /// Resets the entire ATT system, deleting and recreating tables.
        /// </summary>
        /// <param name="tablesToKeep">Tables to keep</param>
        public static void Reset(IEnumerable<string> tablesToKeep)
        {
            string tablesToDrop = DB.Tables.Where(t => tablesToKeep == null || !tablesToKeep.Contains(t)).Concatenate(",");
            if (!string.IsNullOrWhiteSpace(tablesToDrop))
            {
                DB.Connection.ExecuteNonQuery("DROP TABLE " + tablesToDrop + " CASCADE");
                PTL.ATT.Configuration.Reload(true);
            }
        }

        /// <summary>
        /// Reloads the configuration, keeping all existing data.
        /// </summary>
        /// <param name="reinitializeDB">Whether or not to reinitialize the database.</param>
        public static void Reload(bool reinitializeDB)
        {
            _initialized = false;
            Initialize(_path, reinitializeDB);
        }

        private static Assembly LoadExternalFeatureExtractorAssembly(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.Combine(_externalFeatureExtractorDirectory, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                assemblyPath = Path.Combine(_externalFeatureExtractorDirectory, new AssemblyName(args.Name).Name + ".exe");
                if (!File.Exists(assemblyPath))
                    return null;
            }

            return Assembly.LoadFrom(assemblyPath);
        }

        public static bool TryGetFeatureExtractor(Type modelType, out IFeatureExtractor featureExtractor)
        {
            featureExtractor = null;

            Type featureExtractorType;
            if (_modelTypeFeatureExtractorType.TryGetValue(modelType, out featureExtractorType))
                featureExtractor = Activator.CreateInstance(featureExtractorType) as IFeatureExtractor;

            return featureExtractor != null;
        }

        public static Dictionary<string, string> GetFeatureExtractorConfigOptions(Type modelType)
        {
            return _modelTypeFeatureExtractorConfigOptions[modelType];
        }
    }
}
