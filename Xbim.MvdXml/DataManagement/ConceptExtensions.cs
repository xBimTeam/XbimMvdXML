using System.Data;
using Xbim.Common;

namespace Xbim.MvdXml.DataManagement
{
    /// <summary>
    /// Extends mvdConcepts to provide functions for validation and data extraction 
    /// </summary>
    public static class ConceptExtensions
    {
        /// <summary>
        /// Validates the data in the DataTable agains a set of requirements expressed in a Concept.
        /// </summary>
        /// <param name="concept">The class containing the requirement of interest.</param>
        /// <param name="dataTable">The data to validate</param>
        public static bool PassesOn(this Concept concept, DataTable dataTable)
        {
            return concept.TemplateRules.PassesOn(dataTable);
        }

        /// <summary>
        /// Populates a datatable with the identifiers defined from a concept
        /// </summary>
        public static DataTable GetData(this Concept concept, IPersistEntity entity)
        {
            return concept.ParentConceptRoot.ParentModelView.ParentMvdXml.Engine.GetData(entity, concept);
        }
    }
}
