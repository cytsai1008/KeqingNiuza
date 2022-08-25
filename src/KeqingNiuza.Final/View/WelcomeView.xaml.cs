using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KeqingNiuza.View;

/// <summary>
///     WelcomeView.xaml 的交互逻辑
/// </summary>
public partial class WelcomeView : UserControl, INotifyPropertyChanged
{
    private double _DownloadSize;

    private string _DownloadState;


    private int _FileCount;

    private int _HasDownload;


    public WelcomeView()
    {
        InitializeComponent();
    }

    public int FileCount
    {
        get => _FileCount;
        set
        {
            _FileCount = value;
            OnPropertyChanged();
        }
    }

    public int HasDownload
    {
        get => _HasDownload;
        set
        {
            _HasDownload = value;
            OnPropertyChanged();
        }
    }

    public double DownloadSize
    {
        get => _DownloadSize;
        set
        {
            _DownloadSize = value;
            OnPropertyChanged();
        }
    }

    public string DownloadState
    {
        get => _DownloadState;
        set
        {
            _DownloadState = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        var link = sender as Hyperlink;
        Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
    }
}