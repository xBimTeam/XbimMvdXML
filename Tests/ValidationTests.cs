using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Xbim.Ifc;

namespace Tests
{
    [TestClass]
    [DeploymentItem(@"FromMW\", @"FromMW\")]
    public class ValidationTests
    {
        [TestMethod]
        public void ExpectedValidationResults()
        {
            var model = IfcStore.Open(@"FromMW\mvdXML_ifc4_unit-test.ifc");

            // syncpoint
            
            const string mvdName = @"FromMW\mvdXMLUnitTestsforIFC4_2.mvdxml";
            var mvd = mvdXML.LoadFromFile(mvdName);
            var doc = new MvdEngine(mvd, model, false);
            bool app;
            // test 1
            // 
            var cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029095");
            Assert.IsNotNull(cr, "Did not find expected ConceptRoot");
            app = cr.AppliesTo(model.Instances[278]);
            // todo: restore when updated from Matthias
            // Assert.IsTrue(app, "Applicability not matched");

            cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029085");
            Assert.IsNotNull(cr, "Did not find expected ConceptRoot");
            app = cr.AppliesTo(model.Instances[589]);
            Assert.IsTrue(app, "Applicability not matched");
            
            model.Close();
        }
    }
}
