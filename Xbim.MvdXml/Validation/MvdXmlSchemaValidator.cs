using System;
using System.Collections.Generic;
using System.Linq;

namespace Xbim.MvdXml.Validation
{
    /// <summary>
    /// this class supports the identification of invalid content in the mvdXML dataset.
    /// </summary>
    public static class MvdXmlSchemaValidator
    {
        public static IEnumerable<string> ReportIssues(this Concept concept)
        {
            var dataIndicators = concept.TemplateRules?.GetIndicators();
            var s = new HashSet<string>(concept.ConceptTemplate.GetRecursiveRuleIds());
            if (dataIndicators == null)
                yield break;
            foreach (var di in dataIndicators)
            {
                if (!s.Contains(di.VariableName))
                    yield return $"'{di.VariableName}' is not defined among the RuleIds of concept '{concept.uuid}'";
            }
        }

        public static IEnumerable<string> ReportUuidIssues(this mvdXML mvd)
        {
            var dic = mvd.GetUuidDictionary();
            var dupes = dic.Where(x => x.Value.Count > 1);
            foreach (var dupe in dupes)
            {
                var arrTypes = dupe.Value.Select(x => x.GetType().Name).ToArray();
                yield return $"not unique constraint violated for uuid: '{dupe.Key}'; used {dupe.Value.Count} times for {string.Join(",", arrTypes)}.";
            }
        }

        internal static Dictionary<string, List<object>> GetUuidDictionary(this mvdXML mvd)
        {
            var ret = new Dictionary<string, List<object>>();
            AddDic(ret, mvd);
            foreach (var conceptTemplate in mvd.GetAllConceptTemplates())
            {
                AddDic(ret, conceptTemplate);
            }

            foreach (var item in mvd.GetAllConcepts())
            {
                AddDic(ret, item);
            }

            foreach (var item in mvd.GetAllConceptsRoots())
            {
                AddDic(ret, item);
            }

            foreach (var item in mvd.Views)
            {
                AddDic(ret, item);
                foreach (var modelViewExchangeRequirement in item.ExchangeRequirements)
                {
                    AddDic(ret, modelViewExchangeRequirement);
                }
            }
            return ret;
        }

        private static void AddDic(Dictionary<string, List<object>> destDictionary, IUnique item)
        {
            List<object> lst;
            var uuid = item.GetUuid();
            // only add meaningful strings
            //
            if (string.IsNullOrEmpty(uuid))
                return;

            // attempt to get value
            if (destDictionary.TryGetValue(uuid, out lst))
            {
                lst.Add(item);
                return;
            }
            // otherwise create
            destDictionary.Add(uuid, new List<object>() {item});
        }

        public static IEnumerable<string> ReportConceptIssues(this mvdXML mvd)
        {
            foreach (var concept in mvd.GetAllConcepts())
            {
                foreach (var reportIssue in ReportIssues(concept))
                    yield return reportIssue;
            }
        }
    }
}
