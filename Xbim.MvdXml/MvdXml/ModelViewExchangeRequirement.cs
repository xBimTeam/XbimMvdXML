using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xbim.MvdXml
{
    public partial class ModelViewExchangeRequirement : IUnique
    {
        /// <summary>
        /// provides access to the underlying Uuid string
        /// </summary>
        /// <returns>a string</returns>
        public string GetUuid()
        {
            return uuid;
        }
    }
}
