using System.Collections.Generic;

namespace XbimPlugin.MvdXML.Viewing
{
    internal interface ITreeElement
    {
        IEnumerable<ObjectViewModel> GetChildren();
    }
}