using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls;

namespace XbimPlugin.MvdXML
{
    public class FilesystemSuggestionProvider : ISuggestionProvider
    {
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }
            if (filter.Length < 3)
            {
                return null;
            }

            if (filter[1] != ':')
            {
                return null;
            }

            var lst = new List<System.IO.FileSystemInfo>();
            var dirFilter = "*";
            var dirPath = filter;
            if (!filter.EndsWith("\\"))
            {
                var index = filter.LastIndexOf("\\");
                dirPath = filter.Substring(0, index + 1);
                dirFilter = filter.Substring(index + 1) + "*";
            }
            var dirInfo = new DirectoryInfo(dirPath);
            lst.AddRange(dirInfo.GetDirectories(dirFilter));
            lst.AddRange(dirInfo.GetFiles(dirFilter));
            return lst;
        }
    }
}
