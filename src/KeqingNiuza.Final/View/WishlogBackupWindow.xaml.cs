using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using KeqingNiuza.Core.Wish;
using KeqingNiuza.Model;
using KeqingNiuza.Service;
using KeqingNiuza.ViewModel;
using Const = KeqingNiuza.Service.Const;

namespace KeqingNiuza.View;

/// <summary>
///     WishlogBackupWindow.xaml 的交互逻辑
/// </summary>
public partial class WishlogBackupWindow : Window, INotifyPropertyChanged
{
    private bool _ButtonEnable = true;


    private string _RequestInfoText;


    private UserData _SelectedUserData;


    private string _StateInfoText;


    private List<UserData> _UserDataList;


    private List<WishData> _wishlogList;

    public WishlogBackupWindow()
    {
        InitializeComponent();
        UserDataList = MainWindowViewModel.GetUserDataList();
    }

    public List<UserData> UserDataList
    {
        get => _UserDataList;
        set
        {
            _UserDataList = value;
            OnPropertyChanged();
        }
    }

    public UserData SelectedUserData
    {
        get => _SelectedUserData;
        set
        {
            _SelectedUserData = value;
            SelectedUserData_Changed();
            OnPropertyChanged();
        }
    }

    public string StateInfoText
    {
        get => _StateInfoText;
        set
        {
            _StateInfoText = value;
            OnPropertyChanged();
        }
    }

    public string RequestInfoText
    {
        get => _RequestInfoText;
        set
        {
            _RequestInfoText = value;
            OnPropertyChanged();
        }
    }

    public bool ButtonEnable
    {
        get => _ButtonEnable;
        set
        {
            _ButtonEnable = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private void SelectedUserData_Changed()
    {
        if (_SelectedUserData is null) return;
        if (!File.Exists(_SelectedUserData.WishLogFile)) StateInfoText = $"没有找到祈愿记录文件：{_SelectedUserData.WishLogFile}";
        try
        {
            var json = File.ReadAllText(_SelectedUserData.WishLogFile);
            _wishlogList = JsonSerializer.Deserialize<List<WishData>>(json);
            _wishlogList.ForEach(wishlog => wishlog.Uid = _SelectedUserData.Uid);
        }
        catch (Exception ex)
        {
            StateInfoText = $"发生了错误：{ex.Message}";
            return;
        }

        StateInfoText = $"选择了Uid：{_SelectedUserData.Uid}，本地共有祈愿记录{_wishlogList.Count}条";
    }


    private async void Button_Upload_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedUserData == null || _wishlogList == null)
        {
            StateInfoText = "请选择Uid";
            return;
        }

        var client = new WishlogBackupService();
        try
        {
            ButtonEnable = false;
            StateInfoText = "检查祈愿记录网址的有效性";
            await client.EnsureAuthkeyAsync(SelectedUserData.Uid, SelectedUserData.Url);
            StateInfoText = "获取服务器已有数据";
            var result = await client.GetServerDataAsync(SelectedUserData.Uid, SelectedUserData.Url);
            var lastId = result?.List?.LastOrDefault()?.Id;
            var list = _wishlogList.Where(x => x.Id > lastId).ToList();
            if (list.Count == 0)
            {
                StateInfoText = $"本地记录{_wishlogList.Count}条，服务器记录{result?.CurrentCount}条，无需上传";
                return;
            }

            StateInfoText = $"本地记录{_wishlogList.Count}条，服务器已有记录{result?.CurrentCount}条，正在上传祈愿记录";
            result = await client.ExecuteAsync(SelectedUserData.Uid, SelectedUserData.Url, "put", list);
            if (result != null)
            {
                StateInfoText = $"服务器上现有Uid{result.Uid}的祈愿记录{result.CurrentCount}条，此次上传新增{result.PutCount}条";
                return;
            }

            throw new Exception("Result is null");
        }
        catch (Exception ex)
        {
            if (ex.Message == "authkey timeout")
                StateInfoText = "祈愿记录网址已过期，请重新加载数据";
            else
                StateInfoText = $"发生错误：{ex.Message}";
        }
        finally
        {
            ButtonEnable = true;
            RequestInfoText = client.RequestInfo;
        }
    }

    private async void Button_Get_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedUserData == null || _wishlogList == null)
        {
            StateInfoText = "请选择Uid";
            return;
        }

        var client = new WishlogBackupService();
        try
        {
            ButtonEnable = false;
            StateInfoText = "检查祈愿记录网址的有效性";
            await client.EnsureAuthkeyAsync(SelectedUserData.Uid, SelectedUserData.Url);
            StateInfoText = "正在下载，请稍等（最多30s）";
            var result = await client.ExecuteAsync(SelectedUserData.Uid, SelectedUserData.Url, "get", null);
            if (result != null)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "json file|*.json";
                dialog.AddExtension = true;
                dialog.DefaultExt = ".json";
                dialog.FileName = $"WishLog_{result.Uid}_{DateTime.Now:yyyyMMddHHmmss}.json";
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var json = JsonSerializer.Serialize(result.List, Const.JsonOptions);
                    File.WriteAllText(dialog.FileName, json);
                    StateInfoText = $"服务器上现有Uid{result.Uid}的祈愿记录{result.CurrentCount}条，此次下载{result.GetCount}条，保存成功";
                }
                else
                {
                    StateInfoText = $"服务器上现有Uid{result.Uid}的祈愿记录{result.CurrentCount}条，此次下载{result.GetCount}条，但未保存文件";
                }

                return;
            }

            throw new Exception("Result is null");
        }
        catch (Exception ex)
        {
            if (ex.Message == "authkey timeout")
                StateInfoText = "祈愿记录网址已过期，请重新加载数据";
            else
                StateInfoText = $"发生错误：{ex.Message}";
        }
        finally
        {
            ButtonEnable = true;
            RequestInfoText = client.RequestInfo;
        }
    }

    private async void Button_Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedUserData == null || _wishlogList == null)
        {
            StateInfoText = "请选择Uid";
            return;
        }

        var client = new WishlogBackupService();
        try
        {
            ButtonEnable = false;
            StateInfoText = "检查祈愿记录网址的有效性";
            await client.EnsureAuthkeyAsync(SelectedUserData.Uid, SelectedUserData.Url);
            StateInfoText = "正在删除，请稍等（最多30s）";
            var result = await client.ExecuteAsync(SelectedUserData.Uid, SelectedUserData.Url, "delete", null);
            if (result != null)
            {
                StateInfoText = $"服务器上现有Uid{result.Uid}的祈愿记录{result.CurrentCount}条，此次删除{result.DeleteCount}条";
                return;
            }

            throw new Exception("Result is null");
        }
        catch (Exception ex)
        {
            if (ex.Message == "authkey timeout")
                StateInfoText = "祈愿记录网址已过期，请重新加载数据";
            else
                StateInfoText = $"发生错误：{ex.Message}";
        }
        finally
        {
            ButtonEnable = true;
            RequestInfoText = client.RequestInfo;
        }
    }

    private async void Button_UploadAll_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedUserData == null || _wishlogList == null)
        {
            StateInfoText = "请选择Uid";
            return;
        }

        var client = new WishlogBackupService();
        try
        {
            ButtonEnable = false;
            StateInfoText = "检查祈愿记录网址的有效性";
            await client.EnsureAuthkeyAsync(SelectedUserData.Uid, SelectedUserData.Url);
            var steps = _wishlogList.Count / 10000 + 1;
            WishlogResult result = null;
            var addCount = 0;
            for (var i = 0; i < steps; i++)
            {
                var list = _wishlogList.Skip(10000 * i).Take(10000);
                StateInfoText = $"正在上传第{10000 * i + 1}-{10000 * i + list.Count()}条祈愿记录";
                result = await client.ExecuteAsync(SelectedUserData.Uid, SelectedUserData.Url, "put", list);
                addCount += result?.PutCount ?? 0;
            }

            if (result != null)
            {
                StateInfoText = $"服务器上现有Uid{result.Uid}的祈愿记录{result.CurrentCount}条，此次上传新增{addCount}条";
                return;
            }

            throw new Exception("Result is null");
        }
        catch (Exception ex)
        {
            if (ex.Message == "authkey timeout")
                StateInfoText = "祈愿记录网址已过期，请重新加载数据";
            else
                StateInfoText = $"发生错误：{ex.Message}";
        }
        finally
        {
            ButtonEnable = true;
            RequestInfoText = client.RequestInfo;
        }
    }
}