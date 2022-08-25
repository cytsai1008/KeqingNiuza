using System;
using System.IO;

namespace KeqingNiuza.Core.CloudBackup;

internal class BackupFileinfo
{
    public BackupFileinfo(FileInfo fileInfo)
    {
        Name = fileInfo.Name;
        LastUpdateTime = fileInfo.LastWriteTime;
    }

    public BackupFileinfo()
    {
    }

    public string Name { get; set; }

    public DateTime LastUpdateTime { get; set; }
}