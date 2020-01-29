using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xbim.MvdXml.DataManagement;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml 
{
    partial class TemplateRules : ITemplateRule
    {
        IEnumerable<MvdPropertyRuleValue> ITemplateRule.RecursiveProperiesRuleValues()
        {
            return Items?.Cast<ITemplateRule>().SelectMany(item => item.RecursiveProperiesRuleValues());
        }

        internal IEnumerable<ITemplateRule> GetRules()
        {
            if (Items == null)
                return Enumerable.Empty<ITemplateRule>();
            return Items.OfType<ITemplateRule>();
        }

        internal bool PassesOn(DataTable ret)
        {
            return ((ITemplateRule) this).PassesOn(ret);
        }

        bool ITemplateRule.PassesOn(DataTable ret)
        {
            var opString = @operator.ToString().ToLowerInvariant();
            foreach (var templateRule in GetRules().ToArray())
            {
                var value = templateRule.PassesOn(ret);
                if (opString == "or" && value) // value == true
                {
                    return true;
                }
                if (opString == "and" && !value) // value == false
                {
                    return false;
                }
                if (opString == "nor" && value) // value == true
                {
                    return false;
                }
            }
            switch (opString)
            {
                case "and":
                    return true;
                case "or":
                    return false;
                case "nor":
                    return true;
            }
            throw new NotImplementedException(@"PassesOn not implemented for operator: " + opString + " in TemplateRules.");
        }

        internal List<DataIndicator> GetIndicators()
        {
            var ind = ((ITemplateRule)this).RecursiveProperiesRuleValues();
            var all = ind?.Select(x => x.DataIndicator);
            if (all == null)
                return new List<DataIndicator>();
            return all.Distinct().ToList() 
                ?? new List<DataIndicator>();
        }
    }
}
