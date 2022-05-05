using System.Collections.Generic;
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
    [DeploymentItem(@"FromMW\", @"FromMW\")]
    public class ValueExtractionTest
    {
        [TestMethod]
        public void ToleratesMissingFields()
        {
            // ConceptTemplate uuid="10000000-0000-0000-0001-000000000008" has problems with the 
            // use of multiple AttributeRule(s) that do not exist in the schema at that level 
            // We want to tolerate the error and ignore it
            //
            const string fileName = @"FromMW\mvdXMLUnitTestsforIFC4_2.mvdxml";
            var mvd = mvdXML.LoadFromFile(fileName);

            var m = IfcStore.Open(@"FromMW\mvdXML_ifc4_unit-test.ifc");

            // can instantiate engine on model
            var engine = new MvdEngine(mvd, m);
            // data indicators
            var ind = new DataIndicatorLookup("Set", "Property", "Value", "T_Set", "T_Property", "T_Value");

            // simple property data extraction with subtemplates
            //
            var template = mvd.GetConceptTemplate("10000000-0000-0000-0001-000000000008");
            var data = engine.GetAttributes(template, m.Instances[10693], ind, "");
            var str = data.ToString();

            CollectionAssert.AreEqual(data.FieldNames, new List<string>() {"Set", "Property", "Value" });
            Assert.AreEqual(data.Values.Count, 3);
        }

        [TestMethod]
        public void DataExtraction1()
        {
            const string fileName = @"FromMW\mvdXMLUnitTestsforIFC4_2.mvdxml";
            var mvd = mvdXML.LoadFromFile(fileName);

            var m = IfcStore.Open(@"FromMW\mvdXML_ifc4_unit-test.ifc");

            // can instantiate engine on model
            var engine = new MvdEngine(mvd, m);

            // data indicators
            var ind = new DataIndicatorLookup("Set", "Property", "Value", "T_Set", "T_Property", "T_Value");
        
            // simple property data extraction with subtemplates
            //
            var templateSingleProp = mvd.GetConceptTemplate("88b4aaa9-0925-447c-b009-fe357b7c754e");
            var dataSingleProp = engine.GetAttributes(templateSingleProp, m.Instances[606], ind, "");

            CollectionAssert.AreEqual(dataSingleProp.FieldNames, new List<string>() {"Property", "Value"});
            Assert.AreEqual(dataSingleProp.Values.Count, 1);
            Assert.AreEqual(dataSingleProp.Values[0][0].ToString(), "LoadBearing");
            Assert.AreEqual(dataSingleProp.Values[0][1].ToString(), "true");
        }

        [TestMethod]
        public void TestConceptPass()
        {
            const string fileName = @"FromNN\Measurement requirements-1-1RCa.mvdxml";
            var mvd = mvdXML.LoadFromFile(fileName);
            
            var m = IfcStore.Open(@"FromNN\mvdXML test.ifc");

            // can instantiate engine on model
            var engine = new MvdEngine(mvd, m);
            Assert.IsNotNull(engine);

            // m.Open(@"C:\Users\Bonghi\Desktop\str\FILE2015.xBIM");
            // var walls = m.Instances.OfType<IfcWallStandardCase>();
            // var wall = m.Instances[1973];
            var wall = m.Instances.OfType<IfcWallStandardCase>().FirstOrDefault();
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
