using System.Collections.Generic;
using Xbim.MvdXml;

namespace XbimPlugin.MvdXML.Viewing
{
    internal class ExchangeRequirementExpander : ITreeElement
    {
        // ReSharper disable once NotAccessedField.Local
        private ModelViewExchangeRequirement _exchangeRequirement;

        private ExchangeRequirementExpander(ModelViewExchangeRequirement exchangeRequirement)
        {
            _exchangeRequirement = exchangeRequirement;
        }

        IEnumerable<ObjectViewModel> ITreeElement.GetChildren()
        {
            // right now there's no request to expand ER
            yield break;
        }
    }
}
