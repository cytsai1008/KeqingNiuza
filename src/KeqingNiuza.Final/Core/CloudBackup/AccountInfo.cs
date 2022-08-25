using System;

namespace KeqingNiuza.Core.CloudBackup;

internal class AccountInfo
{
    public string Url { get; set; }

    public CloudType CloudType { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public DateTime LastSyncTime { get; set; }
}