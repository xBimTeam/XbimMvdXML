using System.Collections.Generic;
using Xbim.MvdXml;


namespace XbimPlugin.MvdXML.Viewing
{
    class ConceptExpander : ITreeElement
    {
        private readonly Concept _conceptRoot;
        public ConceptExpander(Concept conceptRoot)
        {
            _conceptRoot = conceptRoot;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            if (_conceptRoot.Requirements == null)
                yield break;
            foreach (var child in _conceptRoot.Requirements)
            {
                yield return new ObjectViewModel() {Header = child.exchangeRequirement, Tag = child};
            }
        }
    }
}
