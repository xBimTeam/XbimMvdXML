using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    /// <summary>
    /// Provides extension methods for AttributeRuleEntityRules
    /// </summary>
    public static class AttributeRuleEntityRulesExtensions
    {
        /// <summary>
        /// Transforms the EntityRule[] in an enumerable to simplify code syntax as to prevent null array
        /// </summary>
        public static IEnumerable<EntityRule> NotNullEnumerable(this AttributeRuleEntityRules d)  
        {
            return d?.EntityRule == null
                ? Enumerable.Empty<EntityRule>()
                : d.EntityRule.AsEnumerable();
        }
    }
}
