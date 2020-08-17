using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace EPUBParser
{
    public interface IBaseFile
    {
        string Name { get; set; }
        string FullName { get; set; }
    }
}
