using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace KeqingNiuza.Core.Wish;

public class ImportedWishData : WishData, INotifyPropertyChanged
{
    private string _Comment;

    [JsonIgnore] public bool IsError { get; set; }

    [JsonIgnore]
    public string Comment
    {
        get => _Comment;
        set
        {
            _Comment = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}