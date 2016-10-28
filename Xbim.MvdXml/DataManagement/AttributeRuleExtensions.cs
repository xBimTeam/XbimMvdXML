using System.Collections.Generic;

namespace Xbim.MvdXml.DataManagement
{
    internal static class AttributeRuleExtensions
    {
        public static void DebugTree(this IEnumerable<AttributeRule> rules, int indentation = 0)
        {
            foreach (var rule in rules)
            {
                rule.DebugTree();
            }
        }
    }
}
