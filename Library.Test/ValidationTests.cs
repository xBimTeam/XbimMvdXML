using System.Linq;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Xbim.Ifc;
using Xbim.Ifc4.SharedBldgElements;
using Xunit;
using Shouldly;

namespace Tests
{
    public class ValidationTests
    {
        [Fact]
        public void ExpectedValidationResults()
        {
            var model = IfcStore.Open(@"FromMW\mvdXML_ifc4_unit-test.ifc");

            // syncpoint           
            const string mvdName = @"FromMW\mvdXMLUnitTestsforIFC4_2.mvdxml";
            var mvd = mvdXML.LoadFromFile(mvdName);
            var doc = new MvdEngine(mvd, model);
            bool app;
            // test 1
            // 
            var cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029095");
            cr.ShouldNotBeNull("Did not find expected ConceptRoot");
            app = cr.AppliesTo(model.Instances[278]);

            cr = doc.ConceptRoots.FirstOrDefault(x => x.uuid == "00000023-0000-0000-2000-000000029085");  
            cr.ShouldNotBeNull("Did not find expected ConceptRoot");
            app = cr.AppliesTo(model.Instances[589]);
			app.ShouldBeTrue("Applicability not matched");

			model.Close();
        }


        [Fact]
        public void MinimalWindowTest()
        {
            var model = IfcStore.Open(@"TestFiles\MinimalWindow.ifc");
            var mvd = mvdXML.LoadFromFile(@"TestFiles\MinimalWindow.mvdXML");
            
            var doc = new MvdEngine(mvd, model);
            var conceptRoot = doc.ConceptRoots.FirstOrDefault();
            var window = model.Instances.FirstOrDefault<IfcWindow>();

            conceptRoot.ShouldNotBeNull();
            window.ShouldNotBeNull();

			var app = conceptRoot.AppliesTo(window);
            app.ShouldBeTrue("Applicability not matched");
			
            var result = conceptRoot.Concepts.FirstOrDefault().Test(window, Concept.ConceptTestMode.Raw);
            result.ShouldBe(ConceptTestResult.Pass);
        }
    }
}
