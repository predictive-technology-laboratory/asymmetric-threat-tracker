using NUnit.Framework;
using PTL.ATT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTL.ATT.AttTest.Models
{
    [TestFixture]
    public class FeatureBasedDcmTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
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
