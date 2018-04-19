using System.Collections.Generic;

// ReSharper disable once CheckNamespace 
namespace Xbim.MvdXml
{
    public partial class EntityRule: IReference, IRule
    {
        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public ConceptTemplate ParentConceptTemplate;

        public string Name => EntityName;

        internal void SetParent(ConceptTemplate conceptTemplate)
        {
            ParentConceptTemplate = conceptTemplate;
            foreach (var attributeRule in AttributeRules.NotNullEnumerable())
            {
                attributeRule.SetParent(conceptTemplate);
            }
        }

        public override string ToString() => "EntityRule: " + EntityName;

        private ConceptTemplate GetRefTemplate()
        {
            if (string.IsNullOrEmpty(References?.Template?.@ref))
                return null;
            var refTemplate = ParentConceptTemplate.ParentMvdXml.GetConceptTemplate(References.Template.@ref);
            return refTemplate;
        }

        /// <summary>
        /// Logs debug information about the tree in Log4Net.
        /// </summary>
        /// <param name="indentation">the level of indentation of the current branch</param>
        /// <param name="prefix">prefix for variable names</param>
        internal void DebugTree(int indentation = 0, string prefix = "")
        {
            var refTemplate = GetRefTemplate();
            if (refTemplate != null)
            {
                var tPrefix = prefix;
                if (!string.IsNullOrEmpty(References.IdPrefix))
                    tPrefix = tPrefix + References.IdPrefix;
                // todo: is this indentation + 1?
                refTemplate.DebugTemplateTree(indentation + 1, tPrefix);
            }
            foreach (var attributeRule in AttributeRules.NotNullEnumerable())
            {
                attributeRule.DebugTree(indentation, prefix);
            }
        }

        internal IEnumerable<string> GetRecursiveRuleIds(string prefix)
        {
            var refTemplate = GetRefTemplate();
            if (refTemplate != null)
            {
                var tPrefix = prefix;
                if (!string.IsNullOrEmpty(References.IdPrefix))
                    tPrefix = tPrefix + References.IdPrefix;
                foreach (var tempRuleId in refTemplate.GetRecursiveRuleIds(tPrefix))
                {
                    yield return tempRuleId;
                }
            }

            foreach (var attributeRule in AttributeRules.NotNullEnumerable())
            {
                foreach (var sub in attributeRule.GetRecursiveRuleIds(prefix))
                {
                    yield return sub;
                }
            }
        }

        IEnumerable<ReferenceConstraint> IReference.DirectReferences()
        {
            if (string.IsNullOrEmpty(References?.Template?.@ref))
                yield break;
            yield return new ReferenceConstraint(ParentConceptTemplate, References?.Template?.@ref, typeof(ConceptTemplate));
        }

        IEnumerable<ReferenceConstraint> IReference.AllReferences()
        {
            foreach (var direct in ((IReference)this).DirectReferences())
            {
                yield return direct;
            }
            if (AttributeRules != null)
            {
                foreach (IReference attributeRule in AttributeRules.AttributeRule)
                {
                    foreach (var sub in attributeRule.AllReferences())
                    {
                        yield return sub;
                    }
                }
            }
        }
    }
}