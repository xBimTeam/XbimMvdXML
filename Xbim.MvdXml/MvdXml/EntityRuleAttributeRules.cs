using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    /// <summary>
    /// Provides extension methods for EntityRuleAttributeRules
    /// </summary>
    public static class EntityRuleAttributeRulesExtensions 
    {
        /// <summary>
        /// Transforms the AttributeRule[] in an enumerable to simplify code syntax as to prevent null array
        /// </summary>
        public static IEnumerable<AttributeRule> NotNullEnumerable(this EntityRuleAttributeRules d)
        {
            return d?.AttributeRule == null 
                ? Enumerable.Empty<AttributeRule>() 
                : d.AttributeRule.AsEnumerable();
        }
    }
}
