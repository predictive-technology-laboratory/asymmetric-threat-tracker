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
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PTL.ATT.GUI.Socrata
{
    public class SocrataClient
    {
        public SocrataClient()
        {
        }

        public List<string> GetColumnNames(Uri uri)
        {
            List<string> columnNames = new List<string>();
            foreach (JProperty column in Read(uri, 1).First)
                columnNames.Add(column.Name);

            return columnNames;
        }

        public JToken Read(Uri uri, int numRows)
        {
            try
            {
                uri = new Uri(uri.OriginalString + "?$limit=" + numRows);
                WebRequest request = WebRequest.Create(uri);
                request.Method = "GET";

                using (WebResponse response = request.GetResponse())
                {
                    return JToken.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read Socrata data:  " + ex.Message);
            }
        }
    }
}
