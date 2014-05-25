using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Importers
{
    [Serializable]
    public class FeatureShapefileImporter : ShapefileImporter
    {
        public FeatureShapefileImporter(string name, string path, string sourceURI, IShapefileInfoRetriever shapefileInfoRetriever)
            : base(name, path, sourceURI, shapefileInfoRetriever)
        {
        }
    }
}
