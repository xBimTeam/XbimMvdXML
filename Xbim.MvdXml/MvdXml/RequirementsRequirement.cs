using System.Collections.Generic;
using Xbim.Common;
using Xbim.MvdXml.DataManagement;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    // The use of the attribute requirement is as follows:
    // "mandatory" -> normal behavioud
    // "excluded"  -> then pass and fail are reversed. 
    // "not-relevant" -> it is disabled and need not be checked.
    // 
    // its use in visual styler is as follows:
    // mandatory -> normal behaviour               -> If any fail Red, if all pass green 
    // reccomended -> warning if does not pass     -> If any fail orange, if all pass green 
    // excluded -> pass/fail are reversed          -> If any pass Red, if all fail green 
    // not-relevant (ignore the test, turned off)  -> Invisible (blue in alternative scheme)  (not executed from the enginge)
    // not-recommended ->                          -> if any pass Orange, if all fail green
    // 
    // colours/return statuses need to be interpreted in light of this type (except for not-relevant, that should be avoided to save time)
    // filter in the API to only execute (mandatory, reccomended, excluded, not-rec).

    public partial class RequirementsRequirement : IReference
    {
        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Concept ParentConcept { get; private set; }
        
        internal bool SetParent(Concept concept)
        {
            ParentConcept = concept;
            var tmp = GetExchangeRequirement();
            if (tmp == null) 
                return false;
            tmp.AddPointingConceptRequirement(this);
            return true;
        }

        private ModelViewExchangeRequirement _exchangeRequirement;

        /// <summary>
        /// Allows the navigation of the xml tree to the exchange requirement (the retrival is cached for efficiency).
        /// </summary>
        public ModelViewExchangeRequirement GetExchangeRequirement()
        {
            if (_exchangeRequirement != null)
                return _exchangeRequirement;

            if (ParentConcept?.ParentConceptRoot?.ParentModelView == null)
                return null;

            _exchangeRequirement = ParentConcept.ParentConceptRoot.ParentModelView.GetExchangeRequirement(exchangeRequirement);
            return _exchangeRequirement;
        }

        /// <summary>
        /// Tests the requirement against an entity
        /// </summary>
        /// <param name="ent">The entity to be tested</param>
        /// <returns>In this context all possible values of the return are legitimate (concept can only return Pass/Fail/DoesNotApply)</returns>
        public ConceptTestResult Test(IPersistEntity ent)
        {
            if (requirement == RequirementsRequirementRequirement.notrelevant)
                return ConceptTestResult.DoesNotApply;

            // ret can only be pass/fail/doesnotapply out of this call
            var ret = ParentConcept.Test(ent, Concept.ConceptTestMode.Raw);

            switch (requirement)
            {
                case RequirementsRequirementRequirement.excluded:
                    switch (ret)
                    {
                        case ConceptTestResult.Pass:
                            return ConceptTestResult.Fail;
                        case ConceptTestResult.Fail:
                            return ConceptTestResult.Pass;
                    }
                    return ret;
                case RequirementsRequirementRequirement.recommended:
                    return ret == ConceptTestResult.Fail 
                        ? ConceptTestResult.Warning 
                        : ret;
                case RequirementsRequirementRequirement.notrecommended:
                    return ret == ConceptTestResult.Pass 
                        ? ConceptTestResult.Warning 
                        : ret;
                // ReSharper disable once RedundantCaseLabel
                case RequirementsRequirementRequirement.mandatory:
                // ReSharper disable once RedundantCaseLabel
                case RequirementsRequirementRequirement.notrelevant:
                default:
                    return ret;
            }
        }

        IEnumerable<ReferenceConstraint> IReference.DirectReferences()
        {
            if (string.IsNullOrEmpty(exchangeRequirement))
                yield break;
            yield return new ReferenceConstraint(ParentConcept, exchangeRequirement, typeof(ModelViewExchangeRequirement));
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
