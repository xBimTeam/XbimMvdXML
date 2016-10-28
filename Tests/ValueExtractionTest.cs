using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;
using Xbim.Ifc;

namespace Tests
{
    [TestClass]
    [DeploymentItem(@"FromNN\", @"FromNN\")]
    public class ValueExtractionTest
    {
        [TestMethod]
        public void TestConceptPass()
        {
            const string fileName = @"FromNN\Measurement requirements-1-1RCa.mvdxml";
            var mvd = mvdXML.LoadFromFile(fileName);
            
            var m = IfcStore.Open(@"FromNN\mvdXML test.ifc");
            // m.Open(@"C:\Users\Bonghi\Desktop\str\FILE2015.xBIM");
            // var walls = m.Instances.OfType<IfcWallStandardCase>();
            // var wall = m.Instances[1973];
            var wall = m.Instances.OfType<IfcWallStandardCase>().FirstOrDefault();

            // ReSharper disable once UnusedVariable
            var engine = new MvdEngine(mvd, m, false);
            // can instantiate engine on model

            foreach (var view in mvd.Views)
            {
                foreach (var conceptRoot in view.Roots)
                {
                    Debug.Print("ConceptRoot: "  + conceptRoot.name);
                    foreach (var concept in conceptRoot.Concepts)
                    {
                        var result = concept.Test(wall, Concept.ConceptTestMode.Raw);
                        Debug.Print(concept.name +  " (raw) passes: " + result);
                    }
                }
            }           
            m.Close();
        }
    }
}
