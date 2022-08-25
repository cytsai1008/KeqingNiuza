using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using HandyControl.Controls;
using KeqingNiuza.Properties;
using KeqingNiuza.Service;

namespace KeqingNiuza.View;

/// <summary>
///     SettingView.xaml 的交互逻辑
/// </summary>
public partial class SettingView : UserControl, INotifyPropertyChanged
{
    private bool _DailyCheck_IsAutoCheckIn = Settings.Default.DailyCheck_IsAutoCheckIn;


    private TimeSpan _DailyCheck_RandomDelay = Settings.Default.DailyCheck_RandomDelay;


    private DateTime _DailyCheck_StartTime = Settings.Default.DailyCheck_StartTime;


    private bool _DialyCheck_AlwaysShowResult = Settings.Default.DialyCheck_AlwaysShowResult;


    public SettingView()
    {
        InitializeComponent();
    }

    public bool IsAdmin => ScheduleTask.IsAdmin();


    public bool IsLogonTrigger
    {
        get => Settings.Default.IsLogonTrigger;
        set
        {
            if (ChangeScheduleTask(value, IsGenshinStartTrigger))
            {
                Settings.Default.IsLogonTrigger = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsGenshinStartTrigger
    {
        get => Settings.Default.IsGenshinStartTrigger;
        set
        {
            if (ChangeScheduleTask(IsLogonTrigger, value))
            {
                Settings.Default.IsGenshinStartTrigger = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsOversea
    {
        get => Settings.Default.IsOversea;
        set
        {
            Settings.Default.IsOversea = value;
            OnPropertyChanged();
        }
    }

    public bool DailyCheck_IsAutoCheckIn
    {
        get => _DailyCheck_IsAutoCheckIn;
        set
        {
            _DailyCheck_IsAutoCheckIn = value;
            OnPropertyChanged();
        }
    }

    public DateTime DailyCheck_StartTime
    {
        get => _DailyCheck_StartTime;
        set
        {
            _DailyCheck_StartTime = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan DailyCheck_RandomDelay
    {
        get => _DailyCheck_RandomDelay;
        set
        {
            _DailyCheck_RandomDelay = value;
            OnPropertyChanged();
        }
    }

    public bool DialyCheck_AlwaysShowResult
    {
        get => _DialyCheck_AlwaysShowResult;
        set
        {
            _DialyCheck_AlwaysShowResult = value;
            OnPropertyChanged();
        }
    }


    public bool IsDownloadWallpaper
    {
        get => File.Exists($"{Const.UserDataPath}\\setting_wallpaper");
        set
        {
            if (value)
            {
                Directory.CreateDirectory(Const.UserDataPath);
                File.Create($"{Const.UserDataPath}\\setting_wallpaper").Dispose();
            }
            else
            {
                if (File.Exists($"{Const.UserDataPath}\\setting_wallpaper"))
                    File.Delete($"{Const.UserDataPath}\\setting_wallpaper");
            }

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


    private bool ChangeScheduleTask(bool logon, bool genshinstart)
    {
        try
        {
            var trigger = TaskTrigger.None;
            if (logon) trigger = trigger | TaskTrigger.Logon;
            if (genshinstart) trigger = trigger | TaskTrigger.GenshinStart;
            ScheduleTask.AddTask(trigger);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            Growl.Warning("权限不足");
            return false;
        }
        catch (Exception ex)
        {
            Growl.Warning(ex.Message);
            Log.OutputLog(LogType.Warning, "ChangeScheduleTask", ex);
            return false;
        }
    }


    private void Button_DailyCheckSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DailyCheckTask.AddTask(DailyCheck_IsAutoCheckIn, DailyCheck_StartTime, DailyCheck_RandomDelay);
            Settings.Default.DailyCheck_IsAutoCheckIn = DailyCheck_IsAutoCheckIn;
            Settings.Default.DailyCheck_StartTime = DailyCheck_StartTime;
            Settings.Default.DailyCheck_RandomDelay = DailyCheck_RandomDelay;
            Settings.Default.DialyCheck_AlwaysShowResult = DialyCheck_AlwaysShowResult;
            Growl.Success("保存成功");
        }
        catch (Exception ex)
        {
            Growl.Warning(ex.Message);
            Log.OutputLog(LogType.Warning, "DailyCheckSettingSave", ex);
        }
    }


    private void Button_RealtimeNotes_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("KeqingNiuza.RealtimeNotes.exe");
        }
        catch (Exception ex)
        {
            Log.OutputLog(LogType.Warning, "Run realtimeNotes", ex);
            Growl.Warning(ex.Message);
        }
    }
}