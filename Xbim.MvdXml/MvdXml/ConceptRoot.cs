using System;
using System.Linq;
using log4net;
using Xbim.Common;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public  partial class ConceptRoot
    {
        private static readonly ILog Log = LogManager.GetLogger("XbimMvdXml.ConceptRoot.ConceptRoot");

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
        [System.Xml.Serialization.XmlIgnore()]
        public ModelView ParentModelView { get; private set; }

        internal void SetParent(ModelView modelView)
        {
            ParentModelView = modelView;
            ParentModelView.ParentMvdXml?.Engine?.AddConceptRootLookup(this);
            if (Concepts != null)
            {
                foreach (var concept in Concepts)
                {
                    concept.SetParent(this);
                }
            }
            Applicability?.SetParent(modelView);
        }

        /// <summary>
        /// returns a determination of the applicability of the Concept to a given entity
        /// </summary>
        /// <param name="entity">the Entity of reference.</param>
        /// <returns>false if the passed entity is null</returns>
        public bool AppliesTo(IPersistEntity entity)
        {
            if (string.IsNullOrEmpty(applicableRootEntity))
                return false;
            try
            {
                var filterType = ParentModelView.ParentMvdXml.Engine.GetExpressType(
                    ParentModelView.applicableSchema,
                    applicableRootEntity);
                var contains = filterType.NonAbstractSubTypes.Contains(entity.ExpressType);
                if (!contains)
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error($"applicableRootEntity {applicableRootEntity} not recognised in ConceptRoot '{name}' (uuid: {uuid})", ex);
                return false;
            }

            // now evaluate applicability rules or return true 
            return Applicability == null
                   || Applicability.IsApplicable(entity);
        }
    }
}
