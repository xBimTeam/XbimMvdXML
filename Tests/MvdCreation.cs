using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.MvdXml;

namespace Tests
{
    [TestClass]
    [DeploymentItem("Schema")]
    public class MvdCreation
    {
        //[TestMethod]
        //public void OperatorEnumTest()
        //{
        //    var rules = new TemplateRules { }
        //    var serialiser = new XmlSerializer(typeof(TemplateRules));
        //}

        [TestMethod]
        public void CreateValidationMvdFile()
        {
            const string file = "WallRequirements.mvdXML";
            var mvd = MvdCreationHelper.GetRequirementsMvd();
            mvd.Save(file);

            // file created
            Assert.IsTrue(File.Exists(file));

            // passes schema validation
            var err = ValidateXsd(file);
            if (err != null)
                Debug.WriteLine(err);
            Assert.IsNull(err);
        }

        private string ValidateXsd(string path)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://buildingsmart-tech.org/mvd/XML/1.1", "mvdXML_V1.1.xsd");
            using (var reader = XmlReader.Create(path, new XmlReaderSettings
            {
                Schemas = schemas,
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
            }))
            {
                try
                {
                    var dom = new XmlDocument();
                    dom.Load(reader);
                }
                catch(XmlSchemaValidationException e)
                {
                    return $"[{e.LineNumber}:{e.LinePosition}]: {e.Message}";
                }
            }
            return null;
        }
    }
}
