using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using static KeqingNiuza.Core.CloudBackup.Const;

namespace KeqingNiuza.Core.CloudBackup;

public abstract class CloudClient : INotifyPropertyChanged
{
    private DateTime _LastSyncTime;

    public bool IsJianguo { get; set; }

    public string WebDavUrl { get; set; }

    public string UserName { get; protected set; }

    protected string Password { get; set; }

    public DateTime LastSyncTime
    {
        get => _LastSyncTime;
        set
        {
            _LastSyncTime = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public abstract Task<(bool isSuccessful, int code, string msg)> ConfirmAccount();

    [Obsolete("同步方法逻辑有问题，暂用完整备份代替", true)]
    public abstract Task SyncFiles();

    public abstract Task BackupFileArchive();

    public abstract Task RestoreFileArchive();

    public abstract void SaveEncyptedAccount();


    public static CloudClient Create(string userName, string password, CloudType cloudType, string url = null)
    {
        CloudClient client = null;
        switch (cloudType)
        {
            case CloudType.WebDav:
                if (string.IsNullOrWhiteSpace(url)) url = "https://dav.jianguoyun.com/dav/";
                client = new WebDavBackupClient(userName, password, url);
                break;
        }

        return client;
    }


    public static CloudClient GetClientFromEncryption()
    {
        var bytes = File.ReadAllBytes($"{UserDataPath}\\Account");
        var json = Endecryption.Decrypt(bytes);
        var account = JsonSerializer.Deserialize<AccountInfo>(json, JsonOptions);
        var client = Create(account.UserName, account.Password, account.CloudType, account.Url);
        client.LastSyncTime = account.LastSyncTime;
        return client;
    }
}