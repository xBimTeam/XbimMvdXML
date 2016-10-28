using System.Collections.Generic;
using System.Data;
using Xbim.MvdXml.Validation;

// ReSharper disable once CheckNamespace
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
