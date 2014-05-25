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
        public FeatureShapefileImporter(string name, string path, string sourceURI, int sourceSRID, int targetSRID, IShapefileInfoRetriever shapefileInfoRetriever)
            : base(name, path, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever)
        {
        }
    }
}
