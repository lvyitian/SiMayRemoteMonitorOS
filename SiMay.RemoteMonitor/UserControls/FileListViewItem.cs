using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteMonitor.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UserControls
{
    public class FileListViewItem : ListViewItem
    {
        public FileListViewItem(string fileName, long size, long usingSize, long freeSize, Enums.FileKind type, DateTime lastAccessTime, int imageIndex)
        {
            FileName = fileName;
            FileSize = size;
            UsingSize = usingSize;
            FreeSize = freeSize;
            FileType = type;
            LastAccessTime = lastAccessTime;

            this.Text = fileName;
            this.SubItems.Add(FileHelper.LengthToFileSize((double)size));
            if (type == Enums.FileKind.File)
            {
                var firx = Path.GetExtension(fileName).ToUpper().Replace(".", "");
                this.SubItems.Add(firx + type.GetDescription());
            }
            else
                this.SubItems.Add(type.GetDescription());

            this.SubItems.Add(lastAccessTime.ToString());
            this.ImageIndex = imageIndex;
        }

        public string FileName { get; set; }
        public long FileSize { get; set; }
        public long UsingSize { get; set; }
        public long FreeSize { get; set; }
        public Enums.FileKind FileType { get; set; }
        public DateTime LastAccessTime { get; set; }
    }
}
