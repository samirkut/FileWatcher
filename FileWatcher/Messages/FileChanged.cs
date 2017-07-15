using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Messages
{
    public class FileChanged
    {
        public string FileName { get; set; }
        public string[] Changes { get; set; }
        public bool Replace { get; set; }
    }
}
