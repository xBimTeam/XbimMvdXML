using System.Collections.Generic;
using Xbim.MvdXml;

namespace XbimPlugin.MvdXML.Viewing
{
    internal class ExchangeRequirementViewExpander : ITreeElement
    {
        private readonly ModelView _view;
        public ExchangeRequirementViewExpander(ModelView view)
        {
            _view = view;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            var grp = new ExchangeRequirementGrouping((_view.ExchangeRequirements));
            foreach (var child in grp.GetChildren())
            {
                if (child is ExchangeRequirementGrouping)
                {
                    var childAsGrouping = child as ExchangeRequirementGrouping;
                    yield return new ObjectViewModel() {Header = childAsGrouping.Name, Tag = childAsGrouping};
                }
                else if (child is ModelViewExchangeRequirement)
                {
                    var childAsExchangeRequirement = child as ModelViewExchangeRequirement;
                    yield return new ObjectViewModel() {Header = childAsExchangeRequirement.name, Tag = childAsExchangeRequirement};
                }
            }
        }
    }
}
