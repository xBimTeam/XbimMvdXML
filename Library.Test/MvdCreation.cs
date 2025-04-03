using Shouldly;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Xunit;

namespace Tests
{
    public class MvdCreation
    {
        [Fact]
        public void CreateValidationMvdFile()
        {
            var mvd = MvdCreationHelper.GetRequirementsMvd();
            const string file = "WallRequirements.mvdXML";
            mvd.Save(file);

            // file created
            var t = File.ReadAllText(file);
            File.Exists(file).ShouldBeTrue();

			// passes schema validation
			var err = ValidateXsd(file);
            if (err != null)
                Debug.WriteLine(err);
            err.ShouldBeNull(); 
		}

        private string ValidateXsd(string path)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://buildingsmart-tech.org/mvd/XML/1.1", "Schema\\mvdXML_V1.1.xsd");
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
