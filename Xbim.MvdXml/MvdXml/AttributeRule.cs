using Microsoft.Extensions.Logging;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class AttributeRule: IReference, IRule
    {
        private static readonly ILogger Log = Xbim.Common.XbimLogging.CreateLogger<AttributeRule>();

        /// <summary>
        /// Logs debug information about the tree.
        /// </summary>
        /// <param name="indentation">the level of indentation of the current branch</param>
        /// <param name="prefix">prefix for variable names</param>
        public void DebugTree(int indentation = 0, string prefix = "")
        {
#if DEBUG
            var ind = new string('\t', indentation);
            if (!string.IsNullOrEmpty(RuleID))
                Log.LogDebug($"{ind}{AttributeName} => {RuleID}");
#endif

            foreach (var entityRule in EntityRules.NotNullEnumerable())
            {
#if DEBUG
                Log.LogDebug("{0}{1}{2}{3}", ind, AttributeName,
                    string.IsNullOrEmpty(entityRule.EntityName) // conditional parameters are passed to #2 and #3
                        ? ""
                        : $" ({entityRule.EntityName})",
                    string.IsNullOrEmpty(RuleID)
                        ? ""
                        : $" => {RuleID}"
                    );
#endif
                 entityRule.DebugTree(indentation + 1, prefix);
            }
        }

        public override string ToString()
        {
            return $"{AttributeName}{(string.IsNullOrEmpty(RuleID) ? "" : $" => {RuleID}")}";
        }

        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public ConceptTemplate ParentConceptTemplate { get; private set; }

        public string Name => AttributeName;

        internal void SetParent(ConceptTemplate conceptTemplate)
        {
            // Debug.WriteLine("AttributeRule " + AttributeName);
            ParentConceptTemplate = conceptTemplate;
            foreach (var entityRule in EntityRules.NotNullEnumerable())
            {
                entityRule.SetParent(conceptTemplate);
            }
        }

        internal IEnumerable<string> GetRecursiveRuleIds(string prefix)
        {
            if (!string.IsNullOrEmpty(RuleID))
                yield return prefix + RuleID;
            if (EntityRules?.EntityRule == null)
                yield break;
            foreach (var eRule in EntityRules.EntityRule)
            {
                foreach (var eRuleId in eRule.GetRecursiveRuleIds(prefix))
                {
                    yield return eRuleId;
                }
            }
        }

        IEnumerable<ReferenceConstraint> IReference.DirectReferences()
        {
            yield break;
        }

        IEnumerable<ReferenceConstraint> IReference.AllReferences()
        {
            if (EntityRules == null)
                yield break;
            foreach (IReference attributeRuleEntityRule in EntityRules.EntityRule)
            {
                foreach (var sub in attributeRuleEntityRule.AllReferences())
                {
                    yield return sub;
                }
            }
        }
    }
}
