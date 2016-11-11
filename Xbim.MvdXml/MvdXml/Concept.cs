using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using log4net;
using Xbim.MvdXml.DataManagement;
using Xbim.Common;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public  partial class Concept
    {
        private static readonly ILog Log = LogManager.GetLogger("Xbim.XbimMvdXml.Concept");

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"{name} (uuid: {uuid})";
        }

        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [XmlIgnore()]
        public ConceptRoot ParentConceptRoot { get; private set; }

        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [XmlIgnore()]
        public ConceptTemplate ConceptTemplate { get; private set; }

        [XmlIgnore()]
        private MvdEngine _mvdEngine;

        internal void SetParent(ConceptRoot conceptRoot)
        {
            ParentConceptRoot = conceptRoot;
            for (var i = 0; i < Requirements?.Length; )
            {
                // removes requirements pointing to non-existing exchangerequirements
                var success = Requirements[i].SetParent(this);
                if (!success)
                    Requirements = Requirements.RemoveAt(i);
                else
                    i++;
            }
            
            // try to set the concept template reference.
            if (string.IsNullOrEmpty(Template?.@ref))
                return;
            if (ParentConceptRoot?.ParentModelView?.ParentMvdXml == null)
                return;

            // sets the connection to clear caching event
            _mvdEngine = ParentConceptRoot.ParentModelView.ParentMvdXml.Engine;
            if (_mvdEngine != null)
            {
                _mvdEngine.RequestClearCache += Engine_RequestClearCache;
            }            
            
            ConceptTemplate = ParentConceptRoot.ParentModelView.ParentMvdXml.GetConceptTemplate(Template.@ref);
            if (ConceptTemplate == null)
            {
                Log.WarnFormat($"Concept template {Template.@ref} not found for Concept {ToString()}");
            }
        }

        private void Engine_RequestClearCache()
        {
            _dicCacheRaw.Clear();
            _dicApplies.Clear();
            _dicCacheWithReq.Clear();
        }

        /// <summary>
        /// returns a determination of the applicability of the Concept to a given entity
        /// </summary>
        /// <param name="entity">the Entity of reference.</param>
        /// <returns>true if applicable; returned values are cached for speed.</returns>
        public bool AppliesTo(IPersistEntity entity)
        {
            bool hasIt;
            if (_dicApplies.TryGetValue(entity.EntityLabel, out hasIt))
                return hasIt;

            // it can either apply because of template or Parent ConceptRoot applicableRootEntity
            var ret = ParentConceptRoot.AppliesTo(entity)
                      && ConceptTemplate.AppliesTo(entity);

            _dicApplies.Add(entity.EntityLabel,ret);
            return ret;
        }

        private readonly Dictionary<int, ConceptTestResult> _dicCacheRaw = new Dictionary<int, ConceptTestResult>();
        private readonly Dictionary<int, ConceptTestResult> _dicCacheWithReq = new Dictionary<int, ConceptTestResult>();
        private readonly Dictionary<int, bool> _dicApplies = new Dictionary<int, bool>();

        /// <summary>
        /// Tests whether the TemplateRules of the concept pass on the provided datatable
        /// </summary>
        /// <param name="dataTable">the data to be tested</param>
        public bool PassesOn(DataTable dataTable)
        {
            return TemplateRules.PassesOn(dataTable);
        }

        /// <summary>
        /// Defines the logic to adopt for the concept test in consideration of Requirements specified
        /// </summary>
        public enum ConceptTestMode
        {
            /// <summary>
            /// When used for a test can only return Pass/Fail/DoesNotApply
            /// </summary>
            Raw,
            /// <summary>
            /// all values of the enum can be returned when testing with ThroughRequirementRequirements
            /// </summary>
            ThroughRequirementRequirements
        }
        
        /// <summary>
        /// Tests the concept against an entity
        /// </summary>
        /// <param name="ent">The entity to be tested</param>
        /// <param name="mode">Controls the consideration of Requirements in the test</param>
        /// <returns>
        /// When mode is raw the function can only return Pass/Fail/DoesNotApply, 
        /// all values of the enum can be returned when testing with ThroughRequirementRequirements.
        /// </returns>
        public ConceptTestResult Test(IPersistEntity ent, ConceptTestMode mode)
        {           
#if DEBUG
            Log.Debug($"Concept {uuid} testing {mode}");
#endif
            var entityLabel = ent.EntityLabel;
            ConceptTestResult ret;
            ConceptTestResult hasIt;
            switch (mode)
            {
                case ConceptTestMode.Raw:
                    if (_dicCacheRaw.TryGetValue(entityLabel, out hasIt))
                        return hasIt;
                    if (!AppliesTo(ent))
                    {
                        ret = ConceptTestResult.DoesNotApply;
                    }
                    else
                    {
                        var data = _mvdEngine.GetData(ent, this);
                        var res = PassesOn(data);
                        ret = res
                            ? ConceptTestResult.Pass
                            : ConceptTestResult.Fail;
                    }
#if DEBUG
                    Log.Debug($"Concept {uuid} testing {mode} returning {ret}");
#endif
                    _dicCacheRaw.Add(entityLabel, ret);
                    break;
                // ReSharper disable once RedundantCaseLabel // for readability of code.
                case ConceptTestMode.ThroughRequirementRequirements:
                default:
                    if (_dicCacheWithReq.TryGetValue(entityLabel, out hasIt))
                        return hasIt;
                    ret = (ConceptTestResult)Requirements.Select(x => (int)x.Test(ent)).DefaultIfEmpty(0).Max();
#if DEBUG
                    Log.Debug($"Concept {uuid} testing {mode} returning {ret}");
#endif
                    _dicCacheWithReq.Add(entityLabel, ret);
                    break;
            }
            return ret;
        }
    }
}
