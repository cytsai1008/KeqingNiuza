using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using KeqingNiuza.Service;

namespace KeqingNiuza.Model;

public class UserData : INotifyPropertyChanged, IEquatable<UserData>
{
    private string _Avatar;


    private DateTime _LastUpdateTime;
    public int Uid { get; set; }

    public string Url { get; set; }

    [JsonIgnore] public string WishLogFile => $"{Const.UserDataPath}\\WishLog_{Uid}.json";

    public DateTime LastUpdateTime
    {
        get => _LastUpdateTime;
        set
        {
            _LastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    public string Avatar
    {
        get
        {
            if (File.Exists(_Avatar))
                return _Avatar;
            return "resource\\embed\\Paimon.png";
        }
        set
        {
            _Avatar = value;
            OnPropertyChanged();
        }
    }


    public bool IgnoreFirstStar5Character { get; set; }


    public bool IgnoreFirstStar5Weapon { get; set; }


    public bool IgnoreFirstStar5Permanent { get; set; }

    public bool Equals(UserData other)
    {
        return Uid == other.Uid;
    }


    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }
}