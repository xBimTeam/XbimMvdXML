using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    public partial class ModelViewExchangeRequirement
    {
        /// <summary>
        /// Allows the navigation of the xml tree to the Parent
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public ModelView Parent { get; private set; }

        /// <summary>
        /// A list of concept requirements that us
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public readonly List<RequirementsRequirement> PointingConceptRequirement = new List<RequirementsRequirement>();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"{name} (uuid: {uuid})";
        }

        internal void AddPointingConceptRequirement(RequirementsRequirement pointingRequirement)
        {
            // todo: is there a chance that this is executed multiple times when resetting the structure on a different model?
            if (!PointingConceptRequirement.Contains(pointingRequirement))
                PointingConceptRequirement.Add(pointingRequirement);
        }
        
        internal void SetParent(ModelView mvd)
        {
            Parent = mvd;
        }
    }
}
