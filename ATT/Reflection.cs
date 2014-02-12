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
using System.Reflection;
using System.IO;

namespace PTL.ATT
{
    public static class Reflection
    {
        class ProxyDomain : MarshalByRefObject
        {
            public Assembly LoadFrom(string assemblyPath)
            {
                try { return Assembly.LoadFrom(assemblyPath); }
                catch (Exception ex) { throw ex; }
            }
        }

        private static Dictionary<string, Assembly> _externalTypeAssembly = new Dictionary<string, Assembly>();

        /// <summary>
        /// Gets a type
        /// </summary>
        /// <param name="typeName">Type name in the format "type" or "type@path", where "type" is the name of the type
        /// and "path" is the path to the assembly on disk. Type must have a single parameterless constructor.</param>
        /// <param name="assembly">Assembly to get type from</param>
        /// <returns>Type</returns>
        public static Type GetType(string typeName, Assembly assembly = null)
        {
            string[] typeParts = typeName.Split('@');

            if (typeParts.Length > 1 && assembly != null)
                throw new Exception("Cannot both pass an assembly and include its path in the type name");

            if (assembly == null)
                if (typeParts.Length == 1)
                {
                    int lastPlus = typeParts[0].LastIndexOf('+');
                    if (!_externalTypeAssembly.TryGetValue(typeParts[0], out assembly) &&
                        (lastPlus == -1 || !_externalTypeAssembly.TryGetValue(typeParts[0].Substring(0, lastPlus), out assembly)))
                        assembly = Assembly.GetExecutingAssembly();
                }
                else if (typeParts.Length == 2)
                {
                    assembly = new ProxyDomain().LoadFrom(typeParts[1].Trim());

                    if (!_externalTypeAssembly.ContainsKey(typeParts[0]))
                        _externalTypeAssembly.Add(typeParts[0], assembly);
                }
                else
                {
                    throw new Exception("Invalid type:  " + typeName);
                }

            Type type;
            try
            {
                if (typeParts.Length == 1 || typeParts.Length == 2)
                    type = assembly.GetType(typeParts[0].Trim(), true, false);
                else
                    throw new Exception("Invalid type:  " + typeName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get type:  " + ex);
            }

            return type;
        }
    }
}
