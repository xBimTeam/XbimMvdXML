using System.Xml.XPath;

namespace Validation.mvdXML
{
    public class MvdConceptERReference
    {
        public MvdConceptERReference(MvdXMLDocument mvdXMLDocument, XPathNavigator childNav, string conceptUUID)
        {
            Concept = conceptUUID;
            applicability = childNav.GetAttribute("applicability", "");
            requirement = childNav.GetAttribute("requirement", "");
            exchangeRequirement = childNav.GetAttribute("exchangeRequirement", "");
            mvdXMLDocument.Refs.Add(this);
        }

        public string applicability { get; set; }
        public string requirement { get; set; }
        public string Concept { get; set; }
        public string exchangeRequirement { get; set; }
    }
}
