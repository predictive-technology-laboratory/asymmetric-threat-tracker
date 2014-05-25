using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Importers
{
    public interface IShapefileInfoRetriever
    {
        /// <summary>
        /// Gets shapefile information
        /// </summary>
        /// <param name="shapefilePath">Path to shapefile being imported</param>
        /// <param name="optionValuesToGet">Options for which a value is needed</param>
        /// <param name="optionValue">Dictionary in which to place retrieved option-value pairs</param>
        void GetShapefileInfo(string shapefilePath, List<string> optionValuesToGet, Dictionary<string, string> optionValue);
    }
}
