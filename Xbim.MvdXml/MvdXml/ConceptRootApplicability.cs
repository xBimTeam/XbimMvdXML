using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using log4net;
using Xbim.Common;
using Xbim.MvdXml.DataManagement;
using Xbim.MvdXml.Validation;


// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class ConceptRootApplicability 
    {
        private static readonly ILog Log = LogManager.GetLogger("Xbim.MvdXml.ConceptRootApplicability");

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
                    Log.Error($"Error conceptTemplate {Template.@ref} not found in applicability for {_parentConceptRoot.uuid}.");
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
    }
}
