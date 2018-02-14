using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;

namespace Tests
{
    [TestClass]
    
    public class GrammarTests
    {
        [TestMethod]
        [DeploymentItem(@"GrammarTesting\GrammarTests.xml")]
        [DeploymentItem(@"FromMW\mvdXML_ifc4_unit-test.ifc")]
        public void GrammarForApplication()
        {
            // prepare
            const string mvdName = @"GrammarTests.xml";
            var mvd = mvdXML.LoadFromFile(mvdName);
            using (var model = IfcStore.Open(@"mvdXML_ifc4_unit-test.ifc"))
            {
                var doc = new MvdEngine(mvd, model);
                
                // test
                // 
                var cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "0e93f597-f5e1-475b-87a7-eb007993a50d");
                Assert.IsNotNull(cr, "Did not find expected ConceptRoot");
                var anyWall = model.Instances.OfType<IIfcWall>().FirstOrDefault();
                var applies = cr.AppliesTo(anyWall);
                Assert.IsTrue(applies);
                // wrap up

                model.Close();
            }
            // bye bye
        }

        [TestMethod]
        [DeploymentItem(@"GrammarTesting\GrammarTests.xml")]
        public void t2()
        {
            mvdXML.TestSchemaAdherence(@"GrammarTests.xml");
        }
    }
}
