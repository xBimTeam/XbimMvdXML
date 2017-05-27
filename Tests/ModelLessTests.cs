using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml.Expression;

namespace Tests
{
    [TestClass]
    public class ModelLessGrammarTests
    {
        [TestMethod]
        public void CanParseOne()
        {
            Test("[value] = b");
        }

        [TestMethod]
        public void CanParse()
        {
            // basic structures
            Test("a = b");
            Test("a = true");
            Test("a = 20");
            Test("a = 'string'");
            Test("a = 12.4");
            Test("Fail", false);

            // right can have metric
            Test("a[value] = 20");
            Test("a[value] = b[type]");

            // logical oprations
            Test("(a = b) and (a = true)");
            Test("(a = b) & (a = true)");
            Test("((a = b) & (a = true) NAND (c=3))");
            Test("(a = b) however (a = true)", false);
        }

        private void Test(string testingString, bool expectValid = true)
        {
            var scanner = new Scanner();
            var parser = new Parser(scanner, null);
            scanner.SetSource(testingString, 0);
            
            var res = parser.Parse();
            Assert.AreEqual(res, expectValid, "Unexpected result on ->" + testingString + "<-");
        }
    }
}
