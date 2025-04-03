using Shouldly;
using Xbim.MvdXml;
using Xunit;

namespace Tests
{
    public class IoTests
    {
        [Fact]
        public void CanLoadSampleXmlFile()
        {
            const string fileName = "TestFiles\\mvdXML_V1-1d_test.xml";
            var x = mvdXML.LoadFromFile(fileName);

			x.Templates.Length.ShouldBe(2, "Error reading templates");
            x.Views.Length.ShouldBe(1, "Error reading views");
        }
    }
}
