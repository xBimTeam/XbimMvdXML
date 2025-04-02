using System.Collections.Generic;
using System.Data;
using Xbim.MvdXml.DataManagement;

namespace Xbim.MvdXml
{
    /// <summary>
    ///  simplifies the code for the navigation of rules as defined in the xml
    /// </summary>
    internal interface ITemplateRule
    {
        IEnumerable<MvdPropertyRuleValue> RecursiveProperiesRuleValues();
        
        bool PassesOn(DataTable ret);
    }
}
