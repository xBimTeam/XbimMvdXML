using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Xbim.MvdXml
{
    internal interface IReference
    {
        /// <summary>
        /// Provides the list of references defined at the instance level 
        /// </summary>
        /// <returns>A list of ReferenceConstraints, it could be empty, but should not be null</returns>
        IEnumerable<ReferenceConstraint> DirectReferences();

        /// <summary>
        /// Provides the list of references required for the entity, navigating throgh their data tree
        /// </summary>
        /// <returns>A list of ReferenceConstraints, it could be empty, but should not be null</returns>
        IEnumerable<ReferenceConstraint> AllReferences();
    }
}
