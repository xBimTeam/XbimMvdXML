using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbim.MvdXml;
using XbimPlugin.MvdXML.Viewing;

namespace Tests
{
    [TestClass]
    public class UiTests
    {
        [TestMethod]
        public void TestGrouping()
        {
            var groups = new[]
            {
                new ConceptRoot() {name = "A"},
                new ConceptRoot() {name = "A : A"},
                new ConceptRoot() {name = "B : A"},
                new ConceptRoot() {name = "C : A"},
                new ConceptRoot() {name = "A : A : B"},
                new ConceptRoot() {name = "B : A : B"},
                new ConceptRoot() {name = "1 : 1 : 1"},
                new ConceptRoot() {name = "1 : 2 : 1"},
                new ConceptRoot() {name = "1 : 3 : 1"}
            };

            var g = new ConceptRootGrouping(groups) {GroupOrder = ConceptRootGrouping.GroupOrderMode.TopAtBack};
            foreach (var child in g.GetChildren())
            {
                // Debug.WriteLine(child.ToString());
            }
        }
    }
}
