using NUnit.Framework;
using PTL.ATT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttTest.Models
{
    [TestFixture]
    public class FeatureBasedDcmTest
    {
        [Test]
        public void Test()
        {
            Configuration.Initialize("att_config.xml", true);
            Console.Out.WriteLine("ATT test passed.");
        }
    }
}
