using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xbim.MvdXml
{
    interface IRule
    {
        string RuleID { get; }
        string Name { get; }
    }
}
