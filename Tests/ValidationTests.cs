using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Xbim.Ifc;
using Xbim.Ifc4.SharedBldgElements;

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
            var doc = new MvdEngine(mvd, model);
            // ReSharper disable once JoinDeclarationAndInitializer
            bool app;
            // test 1
            // 
            var cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029095");
            Assert.IsNotNull(cr, "Did not find expected ConceptRoot");
            // ReSharper disable once RedundantAssignment
            app = cr.AppliesTo(model.Instances[278]);
            // todo: restore when updated from Matthias
            // Assert.IsTrue(app, "Applicability not matched");

            cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029085");
            Assert.IsNotNull(cr, "Did not find expected ConceptRoot");
            app = cr.AppliesTo(model.Instances[589]);
            Assert.IsTrue(app, "Applicability not matched");
            
            model.Close();
        }


        [TestMethod]
        [DeploymentItem(@"TestFiles")]
        public void MinimalWindowTest()
        {
            var model = IfcStore.Open(@"MinimalWindow.ifc");
            var mvd = mvdXML.LoadFromFile(@"MinimalWindow.mvdXML");
            
            var doc = new MvdEngine(mvd, model);
            var conceptRoot = doc.ConceptRoots.FirstOrDefault();
            var window = model.Instances.FirstOrDefault<IfcWindow>();

            Assert.IsNotNull(conceptRoot);
            Assert.IsNotNull(window);
            var app = conceptRoot.AppliesTo(window);
            Assert.IsTrue(app, "Applicability not matched");

            var result = conceptRoot.Concepts.FirstOrDefault().Test(window, Concept.ConceptTestMode.Raw);
            Assert.AreEqual(ConceptTestResult.Pass, result);
        }

    }
}
