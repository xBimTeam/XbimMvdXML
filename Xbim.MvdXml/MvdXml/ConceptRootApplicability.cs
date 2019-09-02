using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using Xbim.Common;
using Xbim.MvdXml.DataManagement;


// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class ConceptRootApplicability : IReference
    {
        private static readonly ILogger Log = XbimLogging.CreateLogger<ConceptRootApplicability>();

        /// <summary>
        /// Allows the navigation of the xml tree 
        /// </summary>
        [XmlIgnore()]
        public ModelView ModelView { get; private set; }

        internal void SetParent(ModelView modelView, ConceptRoot parent)
        {
            ModelView = modelView;

            _parentConceptRoot = parent;

            // sets the connection to clear caching event
            var mvdEngine =  ModelView?.ParentMvdXml?.Engine;
            if (mvdEngine != null)
            {
                mvdEngine.RequestClearCache += Engine_RequestClearCache;
            }
        }

        private void Engine_RequestClearCache()
        {
            _dicCache.Clear();
        }

        [XmlIgnore()]
        private ConceptTemplate _conceptTemplate;

        /// <summary>
        /// Allows the navigation to the referenced ConceptTemplate
        /// </summary>
        public ConceptTemplate ConceptTemplate
        {
            get
            {
                if (_conceptTemplate != null)
                {
                    return _conceptTemplate;
                }
                _conceptTemplate = ModelView.ParentMvdXml.GetConceptTemplate(Template.@ref);
                if (_conceptTemplate == null)
                    Log.LogError($"Error conceptTemplate {Template.@ref} not found in applicability for {_parentConceptRoot.uuid}.");
                return _conceptTemplate;
            }
        }

        [XmlIgnore()]
        private ConceptRoot _parentConceptRoot;

        [XmlIgnore()]
        private List<DataIndicator> _dataIndicators;

        [XmlIgnore()]
        internal List<DataIndicator> DataIndicators 
            => _dataIndicators ?? (_dataIndicators = TemplateRules.GetIndicators());

        internal DataTable GetData(IPersistEntity entity)
        {
            var cTemp = ConceptTemplate;
            if (cTemp == null)
            {
                return null;
            }
            return ModelView.ParentMvdXml.Engine.GetData(entity, cTemp, DataIndicators);
        }

        readonly Dictionary<int, bool> _dicCache = new Dictionary<int, bool>();

        internal bool IsApplicable(IPersistEntity entity)
        {
            bool cachedApplicatbilityValue;
            if (_dicCache.TryGetValue(entity.EntityLabel, out cachedApplicatbilityValue))
                return cachedApplicatbilityValue;

            var data = GetData(entity);
            // MvdEngine.DebugDataTable(data);
            var res = TemplateRules.PassesOn(data);
            try
            {
                _dicCache.Add(entity.EntityLabel, res);
            }
            catch
            {
                // ignored (in case of multi-thread?)
            }
            return res;
        }

        IEnumerable<ReferenceConstraint> IReference.DirectReferences()
        {
            if (string.IsNullOrEmpty(Template?.@ref))
                yield break;
            yield return new ReferenceConstraint(_parentConceptRoot, Template?.@ref, typeof(ConceptTemplate));
        }

        IEnumerable<ReferenceConstraint> IReference.AllReferences()
        {
            foreach (var direct in ((IReference)this).DirectReferences())
            {
                yield return direct;
            }
        }
    }
}
