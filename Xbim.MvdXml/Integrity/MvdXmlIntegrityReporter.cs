using System.Collections.Generic;
using System.Linq;

namespace Xbim.MvdXml.Integrity
{
    /// <summary>
    /// this class supports the identification of invalid content in the mvdXML dataset.
    /// </summary>
    public static class MvdXmlIntegrityReporter
    {
        #region ReportVariableNameIssues

        /// <summary>
        /// Tests the existence of required variable names in the templates.
        /// </summary>
        /// <param name="mvd">the mvdXML element to test</param>
        /// <returns>An enumerable of strings with failure feedback; empty if all tests are passed.</returns>
        public static IEnumerable<string> ReportVariableNameIssues(this mvdXML mvd)
        {
            foreach (var concept in mvd.GetAllConcepts())
            {
                foreach (var reportIssue in ReportVariableNameIssues(concept))
                    yield return reportIssue;
            }
        }

        private static IEnumerable<string> ReportVariableNameIssues(this Concept concept)
        {
            var dataIndicators = concept.TemplateRules?.GetIndicators();
            if (concept.ConceptTemplate == null)
                yield break;
            var s = new HashSet<string>(concept.ConceptTemplate.GetRecursiveRuleIds());
            if (dataIndicators == null)
                yield break;

            foreach (var di in dataIndicators)
            {
                if (!s.Contains(di.VariableName))
                    yield return $"'{di.VariableName}' is not defined among the RuleIds of concept '{concept.uuid}'";
            }
        }

        #endregion

        #region ReportUuidIssues

        /// <summary>
        /// Tests for duplicate or missing uuids.
        /// </summary>
        /// <param name="mvd">the mvdXML element to test</param>
        /// <returns>An enumerable of strings with failure feedback; empty if all tests are passed.</returns>
        public static IEnumerable<string> ReportUuidIssues(this mvdXML mvd)
        {
            // unique key violations
            //
            var dicUuidAndType = mvd.GetUuidDictionary();
            var duplicatesdicUuidAndType = dicUuidAndType.Where(x => x.Value > 1);
            foreach (var dupe in duplicatesdicUuidAndType)
            {
                yield return $"not unique constraint violated for {dupe.Key.ReferencedType.Name} uuid: {dupe.Key.ReferencedUuid} (repeated in {dupe.Value} items).";
            }

            // required keys violations  
            //          
            var fullRequired = ((IReference) mvd).AllReferences().GroupBy(x => x.Referenced)
                .Select(group => new {referenced = group.Key, Sources = group.Select(g=>g.Referencing)});
            foreach (var required in fullRequired)
            {
                if (!dicUuidAndType.ContainsKey(required.referenced))
                {
                    yield return $"referenced items missing: {required.referenced.ReferencedType.Name} uuid: {required.referenced.ReferencedUuid} does not exist.";
                    foreach (var source in required.Sources)
                    {
                        yield return $"\treferencing source is: {source.GetType().Name} uuid: {source.GetUuid()}";
                    }
                }
            }
        }

        private static Dictionary<MvdItemReference, int> GetUuidDictionary(this mvdXML mvd)
        {
            var ret = new Dictionary<MvdItemReference, int>();
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

        private static void AddDic(Dictionary<MvdItemReference, int> destDictionary, IUnique item)
        {
            
            var uuid = item.GetUuid();
            // only add meaningful strings
            //
            if (string.IsNullOrEmpty(uuid))
                return;
            var tempRef = new MvdItemReference(item);
            int cnt;
            // attempt to get value
            if (destDictionary.TryGetValue(tempRef, out cnt))
            {
                cnt++;
                return;
            }
            // otherwise create
            destDictionary.Add(tempRef, 1);
        }
        
        #endregion
    }
}
