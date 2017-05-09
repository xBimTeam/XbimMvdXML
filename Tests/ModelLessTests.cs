using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml.Expression;

namespace Tests
{
    [TestClass]
    public class ModelLessTests
    {
        [TestMethod]
        public void CandParseQueryStrings()
        {
            Test("");
        }
        
        private void Test(string testingString)
        {
            var scanner = new Scanner();
            var parser = new Parser(scanner, null);
            scanner.SetSource(testingString, 0);
            var res = parser.Parse();
            Assert.IsTrue(res, "Parser did not succeed on [" + testingString + "]");
        }
    }
}
