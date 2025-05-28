using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model
{
    public class BackupStateEventArgs : EventArgs
    {
        public string BackupName { get; set; }
        public string State { get; set; }
        public int FilesCopied { get; set; }
        public int TotalFiles { get; set; }
        public long SizeCopied { get; set; }
        public long TotalSize { get; set; }
        public int CurrentBackupIndex { get; set; }  // Add this
        public int TotalBackups { get; set; }       // Add this
    }
}
