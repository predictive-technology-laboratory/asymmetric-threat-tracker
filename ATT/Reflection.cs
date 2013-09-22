#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
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
            public Assembly LoadFrom(string AssemblyPath)
            {
                try { return Assembly.LoadFrom(AssemblyPath); }
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
