using NUnit.Framework;
using PTL.ATT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttTest.Importers
{
    [TestFixture]
    public class AreaShapefileImporterTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            Configuration.Reset(null);
        }

        [Test]
        public void Test()
        {
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            DB.Connection.Dispose();
        }
    }
}
