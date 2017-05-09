using System.Collections.Generic;

namespace Xbim.MvdXml.Expression
{
    internal class SelectionStep
    {
        internal SelectionStep(IEnumerable<string> requestVariables)
        {
            RequestVariables = new List<string>(requestVariables);
            ResultCache  = new SelectionResultCache();
        }

        /// <summary>
        /// This list references the dictionary DicVariables at the queryingEnvironment level
        /// </summary>
        internal List<string> RequestVariables;
        /// <summary>
        /// Temporary contents get stored here
        /// </summary>
        internal SelectionResultCache ResultCache;
    }
}
