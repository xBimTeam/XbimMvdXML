using System.Collections.Generic;
using System.Linq;
using log4net;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public  partial class ModelView
    {
        private static readonly ILog Log = LogManager.GetLogger("Xbim.MvdXml.RequirementsRequirement");

        private readonly HashSet<string> _failedLookupMessages = new HashSet<string>();

        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public mvdXML ParentMvdXml { get; private set; }
        
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"{name} (uuid: {uuid})";
        }

        internal void SetParent(mvdXML mvd)
        {
            ParentMvdXml = mvd;

            foreach (var modelViewExchangeRequirement in ExchangeRequirements)
            {
                modelViewExchangeRequirement.SetParent(this);
            }
            foreach (var root in Roots)
            {
                root.SetParent(this);
            }
        }

        internal ModelViewExchangeRequirement GetExchangeRequirement(string exchangeRequirementUuid)
        {
            var ret = ExchangeRequirements?.FirstOrDefault(x => x.uuid == exchangeRequirementUuid);
            if (ret != null) 
                return ret;
            if (_failedLookupMessages.Contains(exchangeRequirementUuid))
                return null;
            _failedLookupMessages.Add(exchangeRequirementUuid);
            Log.Error($"UUID '{exchangeRequirementUuid}' cannot be found in exchange requirements of ModelView '{uuid}'.");
            return null;
        }
    }
}
