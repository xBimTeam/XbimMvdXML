using System.Collections.Generic;
using Xbim.MvdXml;


namespace XbimPlugin.MvdXML.Viewing
{
    class ConceptRootExpander : ITreeElement
    {
        private readonly ConceptRoot _conceptRoot;
        public ConceptRootExpander(ConceptRoot conceptRoot)
        {
            _conceptRoot = conceptRoot;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            var grp = new ConceptGrouping((_conceptRoot.Concepts));
            foreach (var child in grp.GetChildren())
            {
                if (child is ConceptGrouping)
                {
                    var childAsGroup = child as ConceptGrouping;
                    yield return new ObjectViewModel() { Header = childAsGroup.Name, Tag = child };
                }
                else if (child is Concept)
                {
                    var childAsConceptRoot = child as Concept;
                    yield return new ObjectViewModel() { Header = childAsConceptRoot.name, Tag = childAsConceptRoot };
                }
            }
        }
    }
}
