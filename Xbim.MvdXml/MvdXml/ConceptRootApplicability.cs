using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using Xbim.Common;
using Xbim.MvdXml.Validation;


// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class ConceptRootApplicability 
    {
        /// <summary>
        /// Allows the navigation of the xml tree 
        /// </summary>
        [XmlIgnore()]
        public ModelView ModelView { get; private set; }

        internal void SetParent(ModelView modelView)
        {
            ModelView = modelView;
        }

        [XmlIgnore()]
        private ConceptTemplate _conceptTemplate;

        /// <summary>
        /// Allows the navigation to the referenced ConceptTemplate
        /// </summary>
        public ConceptTemplate ConceptTemplate 
            => _conceptTemplate ?? (_conceptTemplate = ModelView.ParentMvdXml.GetConceptTemplate(Template.@ref));
        
        private List<Indicator> _dataIndicators;

        internal List<Indicator> DataIndicators 
            => _dataIndicators ?? (_dataIndicators = TemplateRules.GetIndicators());

        internal DataTable GetData(IPersistEntity entity)
        {
            return ModelView.ParentMvdXml.Engine.GetData(entity, ConceptTemplate, DataIndicators);
        }

        readonly Dictionary<int, bool> _dicCache = new Dictionary<int, bool>();

        internal bool IsApplicable(IPersistEntity entity)
        {
            bool hasIt;
            if (_dicCache.TryGetValue(entity.EntityLabel, out hasIt))
                return hasIt;

            var data = GetData(entity);
            // MvdEngine.DebugDataTable(data);
            var res = TemplateRules.PassesOn(data);
            _dicCache.Add(entity.EntityLabel, res);
            return res;
        }
    }
}
