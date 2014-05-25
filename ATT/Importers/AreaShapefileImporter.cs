using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Importers
{
    [Serializable]
    public class AreaShapefileImporter : ShapefileImporter
    {
        private int _areaContainmentBoxSize;

        public AreaShapefileImporter(string name, string path, string sourceURI, ShapefileImporter.GetShapefileInfoDelegate getShapefileInfo, int areaContainmentBoxSize)
            : base(name, path, sourceURI, getShapefileInfo)
        {
            _areaContainmentBoxSize = areaContainmentBoxSize;
        }

        public override void Import()
        {
            base.Import();

            Area.Create(ImportedShapefile, ImportedShapefile.Name, _areaContainmentBoxSize);
        }

        public override void GetUpdateRequests(UpdateRequestDelegate updateRequest)
        {
            base.GetUpdateRequests(updateRequest);

            updateRequest("Area containment box size (meters)", _areaContainmentBoxSize, null, GetUpdateRequestId("containment_box_size"));
        }

        public override void Update(Dictionary<string, object> updateKeyValue)
        {
            base.Update(updateKeyValue);

            _areaContainmentBoxSize = Convert.ToInt32(updateKeyValue[GetUpdateRequestId("containment_box_size")]);
        }
    }
}
