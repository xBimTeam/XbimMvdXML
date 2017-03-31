using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xbim.MvdXml
{
    interface IUnique
    {
        /// <summary>
        /// provides access to the underlying Uuid string
        /// </summary>
        /// <returns>a string</returns>
        string GetUuid();
    }
}
