using System.Collections.Generic;

namespace Xbim.MvdXml.Validation
{
    public static class MvdXmlValidator
    {
        public static IEnumerable<string> Validate(Concept concept)
        {
            var dataIndicators = concept.TemplateRules.GetIndicators();
            var s = new HashSet<string>(concept.ConceptTemplate.GetRecursiveRuleIds());
            foreach (var di in dataIndicators)
            {
                if (!s.Contains(di.VariableName))
                    yield return $"'{di.VariableName}' is not defined among the RuleIds of concept '{concept.uuid}'";
            }
        }
    }
}
