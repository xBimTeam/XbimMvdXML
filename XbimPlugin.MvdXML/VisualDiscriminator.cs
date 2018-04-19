using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.MvdXml;
using Xbim.MvdXml.DataManagement;


namespace XbimPlugin.MvdXML
{
    class VisualDiscriminator
    {
        private readonly MvdEngine _doc;

        public VisualDiscriminator(MvdEngine document)
        {
            _doc = document;
        }
        
        public enum LayerGroup
        {
            /// <summary>
            /// e.g. spaces if I don't want to see them.
            /// </summary>
            Null = 0,
            /// <summary>
            /// Does not aply
            /// </summary>
            Blue = 1,
            /// <summary>
            /// All passes
            /// </summary>
            Green = 2,
            /// <summary>
            /// At least one warning
            /// </summary>
            Amber = 3,
            /// <summary>
            /// At least one failure
            /// </summary>
            Red = 4           
        }

        public void SetFilters(
            HashSet<Concept> selectedConcepts,
            HashSet<ModelViewExchangeRequirement> selectedExchangeRequirements,
            HashSet<ExpressType> selectedIfcClasses
            )
        {
            _selectedConcepts = selectedConcepts;
            _selectedExchReq = selectedExchangeRequirements;
            _selectedIfcClasses = selectedIfcClasses;
            UseSelection = true;
        }

        private HashSet<Concept> _selectedConcepts;
        private HashSet<ModelViewExchangeRequirement> _selectedExchReq;
        private HashSet<ExpressType> _selectedIfcClasses;

        public bool UseSelection;

        public LayerGroup GetLayerGroup(IPersistEntity ent)
        {
            return !UseSelection 
                ? GetLayerGroupUnfiltered(ent) 
                : GetLayerGroupFiltered(ent);
        }

        public LayerGroup GetLayerGroupUnfiltered(IPersistEntity ent)
        {
            var suitableroots = _doc.GetConceptRoots(ent.ExpressType);
            var ret = 1; // does not apply
            //Debug.WriteLine("Entity label: #" + ent.EntityLabel + " Name:" + ent + " suitableroots: " + suitableroots.Count);
            foreach (var validRoot in suitableroots)
            {
                //Debug.WriteLine(validRoot.uuid  + @" " + validRoot.name);
                foreach (var cpt in validRoot.Concepts)
                {
                    switch (cpt.Test(ent, Concept.ConceptTestMode.ThroughRequirementRequirements))
                    {
                        case ConceptTestResult.Fail:
                            return LayerGroup.Red;  // any fail makes it a fail
                        case ConceptTestResult.Pass:
                            ret = Math.Max(ret, (int) LayerGroup.Green); // if there is a pass then it does apply    
                            break;
                        case ConceptTestResult.Warning:
                            ret = Math.Max(ret, (int)LayerGroup.Amber); // if there is a pass then it does apply    
                            break;
                    }
                }
            }
            return (LayerGroup)ret; // otherwise it does not
        }

        public LayerGroup GetLayerGroupFiltered(IPersistEntity entity)
        {
            var ret = 1; // does not apply
            var thisEntityExpressType = entity.ExpressType;
            if (_selectedIfcClasses.Any())
            {
                // if no one of the selected classes contains the element type in the subtypes skip entity
                if (!_selectedIfcClasses.Any(classToTest => classToTest == thisEntityExpressType || classToTest.NonAbstractSubTypes.Contains(thisEntityExpressType)))
                    return LayerGroup.Null;
            }
            
            var suitableRoots = _doc.GetConceptRoots(thisEntityExpressType);
            foreach (var suitableRoot in suitableRoots)
            {
                if (suitableRoot.Concepts == null)
                    continue;
                foreach (var concept in suitableRoot.Concepts)
                {
                    if (concept.Requirements == null)
                        continue;
                    if (_selectedConcepts.Any())
                    {
                        if (!_selectedConcepts.Contains(concept))
                            continue; // out of concept loop
                    }
                    foreach (var requirementsRequirement in concept.Requirements)
                    {
                        if (_selectedExchReq.Any())
                        {
                            if (!_selectedExchReq.Contains(requirementsRequirement.GetExchangeRequirement()))
                                continue; // out of requirementsRequirement loop
                        }
                        var testResult = requirementsRequirement.Test(entity);

                        switch (testResult)
                        {
                            case ConceptTestResult.Fail:
                                return LayerGroup.Red;  // any fail makes it a fail
                            case ConceptTestResult.Pass:
                                ret = Math.Max(ret, (int)LayerGroup.Green); // if there is a pass 
                                break;
                            case ConceptTestResult.Warning:
                                ret = Math.Max(ret, (int)LayerGroup.Amber); // if there is a warning (higher than green)
                                break;
                        }
                    }
                }
            }
            return (LayerGroup)ret; 
        }
    }
}
