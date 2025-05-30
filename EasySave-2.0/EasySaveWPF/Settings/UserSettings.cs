using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveWPF.Settings
{
    public class UserSettings
    {
        public int SelectedLogFormat { get; set; }
        public string BusinessSoftwareName { get; set; }
        public List<string> EncryptionExtensions { get; set; } = new();
        public string EncryptionKey { get; set; }
        public List<string> PriorityExtensions { get; set; } 

        public string Language { get; set; }
    }
}
