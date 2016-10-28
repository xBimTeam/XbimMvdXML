using System.Collections.Generic;
using System.Linq;
using log4net;
using Xbim.Common;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class ConceptTemplate 
    {
        private static readonly ILog Log = LogManager.GetLogger("Xbim.MvdXml.CncptTemplate");

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"{name} (uuid: {uuid})";
        }

        internal IEnumerable<ConceptTemplate> GetTemplatesTree()
        {
            yield return this;
            if (SubTemplates == null) 
                yield break;
            foreach (var subTemplate in SubTemplates)
            {
                foreach (var sub in subTemplate.GetTemplatesTree())
                {
                    yield return sub;
                }
            }
        }

        internal IEnumerable<string> GetRecursiveRuleIds(string prefix = "")
        {
            if (Rules == null)
                yield break;
            foreach (var attributeRule in Rules)
            {
                foreach (var ruleid in attributeRule.GetRecursiveRuleIds(prefix))
                {
                    yield return ruleid;
                }
            }
        }

        /// <summary>
        /// returns a determination of the applicability of the ConceptTemplate to a given entity
        /// </summary>
        /// <param name="entity">the entity of interest</param>
        /// <returns></returns>
        public bool AppliesTo(IPersistEntity entity)
        {
            if (applicableEntity == null)
                return true; // make an attempt if undefined

            if (applicableEntity.Length != applicableSchema.Length)
            {
                Log.Error($"Applicability array lenght mismatch in ConceptTemplate {uuid}.");
                return false;
            }

            for (var i = 0; i < applicableEntity.Length; i++)
            {
                if (string.IsNullOrEmpty(applicableEntity[i]) || string.IsNullOrEmpty(applicableSchema[i]) )
                    continue;
                var filterType = ParentMvdXml.Engine.GetExpressType(applicableSchema[i], applicableEntity[i]);
                return filterType.NonAbstractSubTypes.Contains(entity.ExpressType);
            }

            return false; // if something is defined then assume no 
        }

        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public mvdXML ParentMvdXml;
        
        internal void SetParent(mvdXML mvd)
        {
            ParentMvdXml = mvd;
            if (Rules != null)
            {
                foreach (var rule in Rules)
                {
                    rule.SetParent(this);
                }
            }
            // ReSharper disable once InvertIf // for code symmetry
            if (SubTemplates != null)
            {
                foreach (var subTemplate in SubTemplates)
                {
                    subTemplate.SetParent(mvd);
                }
            }
        }

        internal void DebugTemplateTree(int indentation = 0, string prefix = "")
        {
            var ind = new string('\t', indentation);
            Log.Debug($"{ind}{ToString()}");
            foreach (var rule in Rules)
            {
                rule.DebugTree(indentation, prefix);
            }
            if (SubTemplates == null)
                return;
            foreach (var subTemplate in SubTemplates)
            {
                subTemplate.DebugTemplateTree(indentation, prefix);
            }
        }
    }
}
