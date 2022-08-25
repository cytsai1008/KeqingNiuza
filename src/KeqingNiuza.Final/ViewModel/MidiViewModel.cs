﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Interop;
using HandyControl.Controls;
using KeqingNiuza.Core.Midi;
using KeqingNiuza.Service;

namespace KeqingNiuza.ViewModel;

public class MidiViewModel : INotifyPropertyChanged
{
    private static MidiPlayer MidiPlayer;
    private readonly IntPtr hWnd;
    private readonly HwndSource hwndSource;


    private bool _CanPlay;


    private bool _IsAdmin;


    private ObservableCollection<MidiFileInfo> _MidiFileInfoList;


    private MidiFileInfo _SelectedMidiFile;


    private bool hotkey;
    private readonly Timer timer;

    public MidiViewModel()
    {
        List<string> files;
        if (Directory.Exists("Resource\\Midi"))
            files = Directory.GetFiles("Resource\\Midi").ToList();
        else
            throw new Exception("Resource\\Midi 文件夹内没有Midi文件");
        if (files.Count == 0) throw new Exception("Resource\\Midi 文件夹内没有Midi文件");
        var infos = files.ConvertAll(x => new MidiFileInfo(x)).OrderBy(x => x.Name);
        MidiFileInfoList = new ObservableCollection<MidiFileInfo>(infos);
        MidiPlayer = new MidiPlayer();
        MidiPlayer.Started += MidiPlayer_Started;
        MidiPlayer.Stopped += MidiPlayer_Stopped;
        MidiPlayer.Finished += MidiPlayer_Finished;
        SelectedMidiFile = MidiFileInfoList.First();
        ChangePlayFile(SelectedMidiFile, false);
        hWnd = Process.GetCurrentProcess().MainWindowHandle;
        hotkey = Util.RegisterHotKey(hWnd);
        hwndSource = HwndSource.FromHwnd(hWnd);
        hwndSource.AddHook(HwndHook);
        RefreshState();
        timer = new Timer(1000);
        timer.AutoReset = true;
        timer.Elapsed += Timer_Elapsed;
    }

    public ObservableCollection<MidiFileInfo> MidiFileInfoList
    {
        get => _MidiFileInfoList;
        set
        {
            _MidiFileInfoList = value;
            OnPropertyChanged();
        }
    }

    public MidiFileInfo SelectedMidiFile
    {
        get => _SelectedMidiFile;
        set
        {
            _SelectedMidiFile = value;
            OnPropertyChanged();
        }
    }

    public bool IsAdmin
    {
        get => _IsAdmin;
        set
        {
            _IsAdmin = value;
            OnPropertyChanged();
        }
    }

    public bool CanPlay
    {
        get => _CanPlay;
        set
        {
            _CanPlay = value;
            OnPropertyChanged();
        }
    }

    public string Name => MidiPlayer.Name;


    public bool IsPlaying
    {
        get => MidiPlayer?.IsPlaying ?? false;
        set
        {
            MidiPlayer.IsPlaying = value;
            OnPropertyChanged();
        }
    }


    public bool AutoSwitchToGenshinWindow
    {
        get => MidiPlayer.AutoSwitchToGenshinWindow;
        set
        {
            MidiPlayer.AutoSwitchToGenshinWindow = value;
            OnPropertyChanged();
        }
    }


    public bool PlayBackground
    {
        get => MidiPlayer.PlayBackground;
        set
        {
            MidiPlayer.PlayBackground = value;
            OnPropertyChanged();
        }
    }

    public double Speed
    {
        get => MidiPlayer.Speed;
        set
        {
            MidiPlayer.Speed = value;
            timer.Interval = 1000 / value;
            OnPropertyChanged();
        }
    }

    public int NoteLevel
    {
        get => MidiPlayer.NoteLevel;
        set
        {
            MidiPlayer.NoteLevel = value;
            RefreshMidiFileInfoByNoteLevel(value);
            OnPropertyChanged();
        }
    }

    public TimeSpan TotalTime => MidiPlayer.TotalTime;


    public TimeSpan CurrentTime
    {
        get => MidiPlayer.CurrentTime;
        set
        {
            MidiPlayer.CurrentTime = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public void RefreshMidiFileInfoByNoteLevel(int noteLevel)
    {
        foreach (var item in MidiFileInfoList) item.RefreshTracksByNoteLevel(noteLevel);
        var info = SelectedMidiFile;
        SelectedMidiFile = null;
        SelectedMidiFile = info;
    }


    private void MidiPlayer_Started(object sender, EventArgs e)
    {
        timer.Start();
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("CurrentTime");
    }

    private void MidiPlayer_Stopped(object sender, EventArgs e)
    {
        timer.Stop();
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("CurrentTime");
    }

    private void MidiPlayer_Finished(object sender, EventArgs e)
    {
        timer.Stop();
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("CurrentTime");
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("CurrentTime");
    }


    public void ChangePlayFile(MidiFileInfo info, bool autoPlay = true)
    {
        MidiPlayer.ChangeFileInfo(info, autoPlay);
        OnPropertyChanged("Name");
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("AutoSwitchToGenshinWindow");
        OnPropertyChanged("PlayBackground");
        OnPropertyChanged("Speed");
        OnPropertyChanged("NoteLevel");
        OnPropertyChanged("TotalTime");
        OnPropertyChanged("CurrentTime");
    }

    public void ChangeMidiTrack()
    {
        timer.Stop();
        MidiPlayer.Started -= MidiPlayer_Started;
        MidiPlayer.Stopped -= MidiPlayer_Stopped;
        MidiPlayer.Finished -= MidiPlayer_Finished;
        MidiPlayer?.ChangeFileInfo();
        MidiPlayer.Started += MidiPlayer_Started;
        MidiPlayer.Stopped += MidiPlayer_Stopped;
        MidiPlayer.Finished += MidiPlayer_Finished;
        timer.Start();
    }


    public void RefreshState()
    {
        IsAdmin = Util.IsAdmin();
        CanPlay = MidiPlayer.CanPlay;
        StateText = "正常";
        TextBlock_Color = "Black";
        Button_Restart_Content = null;
        Tooltip_Content = null;
        if (!hotkey)
        {
            StateText = "热键定义失败";
            TextBlock_Color = "Red";
            Button_Restart_Content = "重试";
            Tooltip_Content = null;
        }

        if (!CanPlay)
        {
            StateText = "没有找到原神的窗口";
            TextBlock_Color = "Red";
            Button_Restart_Content = "刷新";
            Tooltip_Content = "请打开游戏后点击刷新";
        }

        if (!IsAdmin)
        {
            StateText = "需要管理员权限";
            TextBlock_Color = "Red";
            Button_Restart_Content = "重启";
            Tooltip_Content = "软件会用管理员权限干什么？\n《原神》以管理员权限启动，软件需要管理员权限才能向游戏窗口发送键盘信息";
        }
    }


    public void RestartOrRefresh()
    {
        if (!IsAdmin)
            try
            {
                Util.RestartAsAdmin();
            }
            catch (Exception ex)
            {
                Log.OutputLog(LogType.Error, "RestartAsAdmin", ex);
                Growl.Error("无法重启，请手动以管理员权限启动");
            }

        if (!CanPlay)
        {
            MidiPlayer = new MidiPlayer();
            SelectedMidiFile = MidiFileInfoList.First();
            RefreshState();
        }

        if (!hotkey)
        {
            _ = Util.UnregisterHotKey(hWnd);
            hotkey = Util.RegisterHotKey(hWnd);
            RefreshState();
        }
    }


    private IntPtr HwndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_KOTKEY = 0x0312;
        if (msg == WM_KOTKEY)
            switch (wParam.ToInt32())
            {
                // 播放/暂停
                case 1000:
                    if (IsPlaying)
                    {
                        IsPlaying = false;
                    }
                    else
                    {
                        if (MidiPlayer?.MidiFileInfo == null)
                            ChangePlayFile(MidiFileInfoList.First());
                        else
                            IsPlaying = true;
                    }

                    handled = true;
                    break;
                // 上一首
                case 1001:
                    PlayLast();
                    handled = true;
                    break;
                // 下一首
                case 1002:
                    PlayNext();
                    handled = true;
                    break;
            }

        return IntPtr.Zero;
    }


    public void PlayLast()
    {
        if (MidiPlayer.MidiFileInfo == null)
        {
            ChangePlayFile(MidiFileInfoList.Last());
        }
        else
        {
            var index = MidiFileInfoList.IndexOf(MidiPlayer.MidiFileInfo);
            if (index == 0)
                ChangePlayFile(MidiFileInfoList.Last());
            else
                ChangePlayFile(MidiFileInfoList[index - 1]);
        }
    }


    public void PlayNext()
    {
        if (MidiPlayer.MidiFileInfo == null)
        {
            ChangePlayFile(MidiFileInfoList.First());
        }
        else
        {
            var index = MidiFileInfoList.IndexOf(MidiPlayer.MidiFileInfo);
            if (index == MidiFileInfoList.Count - 1)
                ChangePlayFile(MidiFileInfoList.First());
            else
                ChangePlayFile(MidiFileInfoList[index + 1]);
        }
    }

    #region ControlProperties

    private string _StateText;

    public string StateText
    {
        get => _StateText;
        set
        {
            _StateText = value;
            OnPropertyChanged();
        }
    }


    private string _Button_Restart_Content;

    public string Button_Restart_Content
    {
        get => _Button_Restart_Content;
        set
        {
            _Button_Restart_Content = value;
            OnPropertyChanged();
        }
    }


    private string _TextBlock_Color;

    public string TextBlock_Color
    {
        get => _TextBlock_Color;
        set
        {
            _TextBlock_Color = value;
            OnPropertyChanged();
        }
    }


    private string _Tooltip_Content;

    public string Tooltip_Content
    {
        get => _Tooltip_Content;
        set
        {
            _Tooltip_Content = value;
            OnPropertyChanged();
        }
    }

    #endregion
}