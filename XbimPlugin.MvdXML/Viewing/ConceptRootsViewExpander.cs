using System.Collections.Generic;
using Xbim.MvdXml;

namespace XbimPlugin.MvdXML.Viewing
{
    internal class ConceptRootsViewExpander : ITreeElement
    {
        private readonly ModelView _view;
        public ConceptRootsViewExpander(ModelView view)
        {
            _view = view;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            var grp = new ConceptRootGrouping((_view.Roots));
            foreach (var child in grp.GetChildren())
            {
                if (child is ConceptRootGrouping)
                {
                    var childAsGroup = child as ConceptRootGrouping;
                    yield return new ObjectViewModel() {Header = childAsGroup.Name, Tag = child};
                }
                else if (child is ConceptRoot)
                {
                    var childAsConceptRoot = child as ConceptRoot;
                    yield return new ObjectViewModel() { Header = childAsConceptRoot.name, Tag = new ConceptRootExpander(childAsConceptRoot) };
                }
            }
            
        }
    }
}
