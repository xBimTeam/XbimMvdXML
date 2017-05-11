using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml.Expression;

namespace Tests
{
    [TestClass]
    public class ModelLessTests
    {
        [TestMethod]
        public void CanParseOne()
        {
            //Test("a = b");
            //Test("a = true");
            Test("a[value] = 20");
                
            //Test("Fail", false);
        }

        [TestMethod]
        public void CanParse()
        {
            Test("a = b");
            Test("a = true");
            Test("a = 20");
            Test("a = 'string'");
            Test("a = 12.4");
            Test("Fail", false);
        }

        private void Test(string testingString, bool expectValid = true)
        {
            var scanner = new Scanner();
            var parser = new Parser(scanner, null);
            scanner.SetSource(testingString, 0);
            
            var res = parser.Parse();
            Assert.AreEqual(res, expectValid, "Unexpected result on [" + testingString + "]");
        }
    }
}
