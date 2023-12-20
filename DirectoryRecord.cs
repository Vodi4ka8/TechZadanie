using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechZadanie
{
    public class DirectoryRecord
    {
        
        public DirectoryInfo GetProjectPath()
        {
            var path = Directory.GetCurrentDirectory();
            int i = 3;
            while (i != 0)
            {
                path = Directory.GetParent(path).FullName;
                i--;
            }
            return new DirectoryInfo(path);
        }
    }
}
