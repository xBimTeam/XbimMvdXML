using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Xbim.MvdXml.DataManagement;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class TemplateRulesTemplateRule : ITemplateRule
    {
        IEnumerable<MvdPropertyRuleValue> ITemplateRule.RecursiveProperiesRuleValues()
        {
            return MvdPropertyRuleValue.GetValues(Parameters);
        }

        bool ITemplateRule.PassesOn(DataTable ret)
        {
            if (ret == null)
                return false;

            var thisLevelVal = MvdPropertyRuleValue.BuildSql(Parameters, ret);
            try
            {
                var v = ret.Select(thisLevelVal).Any();
                return v;
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Xbim.MvdXml.TemplateRulesTemplateRule");
                log.Error($"Problem in parameters field \"{Parameters}\" for templaterule (Description: \"{Description}\").", ex);
                return false;
            }
        }
    }
}
