﻿#region copyright
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
using LAIR.Collections.Generic;
using PTL.ATT.GUI.Plugins;
using PTL.ATT.GUI.Properties;
using LAIR.Extensions;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Configuration;

namespace PTL.ATT.GUI
{
    public static class Configuration
    {
        #region logging
        private static string _logPath;

        public static string LogPath
        {
            get { return Configuration._logPath; }
            set { Configuration._logPath = value; }
        }

        private static string _loggingEditor;

        public static string LoggingEditor
        {
            get { return Configuration._loggingEditor; }
            set { Configuration._loggingEditor = value; }
        }
        
        #endregion

        #region plugins
        private static Set<Plugin> _pluginTypes;

        public static Set<Plugin> PluginTypes
        {
            get { return Configuration._pluginTypes; }
            set { Configuration._pluginTypes = value; }
        }
        #endregion

        #region notifications
        private static string _notificationHost;

        public static string NotificationHost
        {
            get { return Configuration._notificationHost; }
            set { Configuration._notificationHost = value; }
        }
        private static int _notificationPort;

        public static int NotificationPort
        {
            get { return Configuration._notificationPort; }
            set { Configuration._notificationPort = value; }
        }
        private static bool _notificationEnableSSL;

        public static bool NotificationEnableSSL
        {
            get { return Configuration._notificationEnableSSL; }
            set { Configuration._notificationEnableSSL = value; }
        }
        private static string _notificationUsername;

        public static string NotificationUsername
        {
            get { return Configuration._notificationUsername; }
            set { Configuration._notificationUsername = value; }
        }
        private static byte[] _notificationPasswordEncryptedBytes;

        public static string NotificationPassword
        {
            get { return Configuration._notificationPasswordEncryptedBytes.Decrypt(EncryptionKey, EncryptionInitialization); }
        }
        private static string _notificationFromEmail;

        public static string NotificationFromEmail
        {
            get { return Configuration._notificationFromEmail; }
            set { Configuration._notificationFromEmail = value; }
        }
        private static string _notificationFromName;

        public static string NotificationFromName
        {
            get { return Configuration._notificationFromName; }
            set { Configuration._notificationFromName = value; }
        }
        private static List<Tuple<string, string>> _notificationToEmailNames;

        public static List<Tuple<string, string>> NotificationToEmailNames
        {
            get { return Configuration._notificationToEmailNames; }
            set { Configuration._notificationToEmailNames = value; }
        }
        #endregion

        #region encryption
        private static byte[] _encryptionKey;

        public static byte[] EncryptionKey
        {
            get
            {
                if (_encryptionKey == null)
                    SetEncryptionKey(false);

                return Configuration._encryptionKey;
            }
        }

        private static byte[] _encryptionInitialization;

        public static byte[] EncryptionInitialization
        {
            get { return Configuration._encryptionInitialization; }
        }
        #endregion

        #region mono hacks
        private static bool _monoAddIncidentRefresh;

        public static bool MonoAddIncidentRefresh
        {
            get { return Configuration._monoAddIncidentRefresh; }
            set { Configuration._monoAddIncidentRefresh = value; }
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

        public static void Initialize(string path)
        {
            XmlParser p = new XmlParser(File.ReadAllText(path));

            string applicationDataDirectory = p.ElementText("application_data_directory");
            if (string.IsNullOrWhiteSpace(applicationDataDirectory))
                applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "att");

            if(!Directory.Exists(applicationDataDirectory))
                Directory.CreateDirectory(applicationDataDirectory);

            string encryptionInitPath = Path.Combine(applicationDataDirectory, "encryption_initialization");
            if (File.Exists(encryptionInitPath))
                _encryptionInitialization = File.ReadAllText(encryptionInitPath).Split('-').Select(s => byte.Parse(s)).ToArray();
            else
            {
                _encryptionInitialization = new byte[16];
                RandomNumberGenerator.Create().GetBytes(_encryptionInitialization);
                File.WriteAllText(encryptionInitPath, string.Join("-", _encryptionInitialization));
            }

            XmlParser loggingP = new XmlParser(p.OuterXML("logging"));
            _loggingEditor = loggingP.ElementText("editor");
            _logPath = Path.Combine(applicationDataDirectory, "log.txt");

            _pluginTypes = new Set<Plugin>();
            XmlParser pluginsP = new XmlParser(p.OuterXML("plugins"));
            string pluginTypeStr;
            while ((pluginTypeStr = pluginsP.ElementText("plugin")) != null)
                _pluginTypes.Add(Activator.CreateInstance(Reflection.GetType(pluginTypeStr)) as Plugin);

            XmlParser notificationsP = new XmlParser(p.OuterXML("notifications"));
            XmlParser notificationsSetupP = new XmlParser(notificationsP.OuterXML("setup"));
            _notificationHost = notificationsSetupP.ElementText("host");
            if (!string.IsNullOrWhiteSpace(_notificationHost))
            {
                _notificationPort = int.Parse(notificationsSetupP.ElementText("port"));
                _notificationEnableSSL = bool.Parse(notificationsSetupP.ElementText("enable_ssl"));
                _notificationUsername = notificationsSetupP.ElementText("username");

                _notificationPasswordEncryptedBytes = notificationsSetupP.ElementText("password").Split('-').Select(b => byte.Parse(b)).ToArray();

                XmlParser fromP = new XmlParser(notificationsSetupP.OuterXML("from"));
                _notificationFromEmail = fromP.ElementText("email");
                _notificationFromName = fromP.ElementText("name");

                _notificationToEmailNames = new List<Tuple<string, string>>();
                XmlParser toP = new XmlParser(notificationsP.OuterXML("to"));
                string addressXML;
                while ((addressXML = toP.OuterXML("address")) != null)
                {
                    XmlParser addressP = new XmlParser(addressXML);
                    _notificationToEmailNames.Add(new Tuple<string, string>(addressP.ElementText("email"), addressP.ElementText("name")));
                }
            }

            XmlParser monoHacksP = new XmlParser(p.OuterXML("mono_hacks"));
            _monoAddIncidentRefresh = bool.Parse(monoHacksP.ElementText("add_incident_refresh"));

            XmlParser systemP = new XmlParser(p.OuterXML("system"));
            _processorCount = int.Parse(systemP.ElementText("processor_count"));
            if (_processorCount == -1)
                _processorCount = Environment.ProcessorCount;
        }

        public static void SetEncryptionKey(bool reset)
        {
            if (_encryptionKey != null && !reset)
                return;

            string passphraseHashPath = ".encryption_passphrase_hash";
            if (reset)
                File.WriteAllText(passphraseHashPath, "");

            string passphraseHash = "";
            if (File.Exists(passphraseHashPath))
                passphraseHash = File.ReadAllText(passphraseHashPath);

            string key = null;

            while (true)
            {
                DynamicForm f = new DynamicForm("Enter encryption passphrase", DynamicForm.CloseButtons.OkCancel);
                f.AddTextBox("Passphrase:", null, 20, "passphrase", '*', true);
                f.AddTextBox("Confirm passphrase:", null, 20, "confirmed", '*', true);

                if (f.ShowDialog() == DialogResult.Cancel)
                    break;

                string passphrase = f.GetValue<string>("passphrase").Trim();
                string confirmed = f.GetValue<string>("confirmed").Trim();
                if (passphrase != confirmed)
                    MessageBox.Show("Entries do not match.");
                else if (passphrase.Length < 8)
                    MessageBox.Show("Passphrase must be at least 8 characters.");
                else
                {
                    key = passphrase;
                    break;
                }
            }

            if (key == null)
                throw new Exception("Encryption not set up. No passphrase supplied.");

            _encryptionKey = new byte[32];
            byte[] keyBytes = Encoding.Default.GetBytes(key);
            Array.Copy(keyBytes, _encryptionKey, keyBytes.Length);

            string hash;
            using (SHA256 sha2 = SHA256Managed.Create())
            {
                hash = sha2.ComputeHash(_encryptionKey).Select(b => b.ToString()).Concatenate("-");
            }

            if (passphraseHash == "")
                File.WriteAllText(passphraseHashPath, hash);
            else if (hash != passphraseHash)
            {
                _encryptionKey = null;
                throw new Exception("The given passphrase does not match the one previously established. Encryption will not function properly. To enable encryption, either enter the correct passphrase or reset the passphrase with File -> Reset encryption key.");
            }
        }
    }
}
