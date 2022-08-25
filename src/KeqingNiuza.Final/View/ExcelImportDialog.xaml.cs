using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Interactivity;
using HandyControl.Tools.Extension;
using KeqingNiuza.Core.Wish;
using KeqingNiuza.Model;
using KeqingNiuza.Service;
using Application = System.Windows.Application;
using Const = KeqingNiuza.Service.Const;
using UserControl = System.Windows.Controls.UserControl;

namespace KeqingNiuza.View;

/// <summary>
///     ExcelImportView.xaml 的交互逻辑
/// </summary>
public partial class ExcelImportDialog : UserControl, INotifyPropertyChanged, IDialogResultable<bool>
{
    private List<WishData> _ImportedWishDatas;


    private int _ImportUid;


    private int _StartRow = 1;

    public ExcelImportDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public int ImportUid
    {
        get => _ImportUid;
        set
        {
            _ImportUid = value;
            OnPropertyChanged();
        }
    }

    public List<WishData> ImportedWishDatas
    {
        get => _ImportedWishDatas;
        set
        {
            _ImportedWishDatas = value;
            OnPropertyChanged();
        }
    }

    public int StartRow
    {
        get => _StartRow;
        set
        {
            _StartRow = value;
            OnPropertyChanged();
        }
    }


    public bool Result { get; set; }
    public Action CloseAction { get; set; }
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


    private void Button_SelectFile_Click(object sender, RoutedEventArgs e)
    {
        var item = ComboBox_ImportTemplate.SelectedItem as ComboBoxItem;
        var tag = item?.Tag as string;
        var old = tag?.Contains("old") ?? false;
        var filter = tag == "json" ? "Json file|*.json" : "Excel worksheets|*.xlsx";
        var openFileDialog = new OpenFileDialog
        {
            Filter = $"{filter}|All|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
            try
            {
                var file = openFileDialog.FileName;
                if (Path.GetExtension(file) == ".xlsx")
                {
                    ImportExcelFile(file, old);
                    TextBlock_Info.Foreground = new SolidColorBrush(Colors.Gray);
                    TextBlock_Info.Text = openFileDialog.SafeFileName;
                    return;
                }

                if (Path.GetExtension(file) == ".json")
                {
                    ImportJsonFile(file);
                    TextBlock_Info.Foreground = new SolidColorBrush(Colors.Gray);
                    TextBlock_Info.Text = openFileDialog.SafeFileName;
                    return;
                }

                TextBlock_Info.Text = "拓展名不正确";
                TextBlock_Info.Foreground = new SolidColorBrush(Colors.Red);
            }
            catch (Exception ex)
            {
                TextBlock_Info.Text = "无法解析文件";
                TextBlock_Info.Foreground = new SolidColorBrush(Colors.Red);
                Growl.Warning(ex.Message);
                Log.OutputLog(LogType.Error, "Button_SelectFile_Click", ex);
            }
    }

    private void DataGrid_ImportedData_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        var row = e.Row.GetIndex();
        e.Row.Header = row + 1;
    }

    private void Button_Import_Click(object sender, RoutedEventArgs e)
    {
        if (ImportUid == 0)
        {
            TextBlock_Error.Text = "请输入Uid";
            return;
        }

        if (StartRow > ImportedWishDatas.Count)
        {
            TextBlock_Error.Text = "跳过的行数过多";
            return;
        }

        try
        {
            ExportWishLogList();
            Result = true;
            var user = new UserData {Uid = ImportUid, LastUpdateTime = DateTime.Now};
            (Application.Current.MainWindow as MainWindow).ViewModel.SelectedUserData = user;
            ControlCommands.Close.Execute(null, this);
        }
        catch (Exception ex)
        {
            TextBlock_Error.Text = ex.Message;
            Log.OutputLog(LogType.Error, "ImportWishLog", ex);
        }
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        ControlCommands.Close.Execute(null, this);
    }


    private void ImportExcelFile(string path, bool old)
    {
        if (old)
        {
            var list = ExcelImporter.ImportFromExcel(path);
            list.Reverse();
            ImportedWishDatas = list;
        }
        else
        {
            var list = UIGFExcelImporter.Import(path);
            ImportedWishDatas = list.Distinct().OrderByDescending(x => x.Id).ToList();
            ImportUid = ImportedWishDatas[0].Uid;
        }
    }


    private void ImportJsonFile(string path)
    {
        var json = File.ReadAllText(path);
        var list = new JsonImporter().Deserialize(json);
        ImportedWishDatas = list.Distinct().OrderByDescending(x => x.Id).ToList();
        if (ImportedWishDatas?.Any() ?? false) ImportUid = ImportedWishDatas[0].Uid;
    }


    private void ExportWishLogList()
    {
        var list = ImportedWishDatas.Skip(StartRow - 1).ToList();
        if (list.Any())
            foreach (var item in list)
                if (item.Uid == 0)
                    item.Uid = ImportUid;
        // 没有id则从10..01开始赋值
        if (list.Any() && list[0].Id == 0)
        {
            // 重新以时间正序排列
            list.Reverse();
            var id = 1000000000000000001;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.Id == 0) item.Id = id + i;
            }
        }

        var fileName = $"{Const.UserDataPath}\\WishLog_{ImportUid}.json";
        if (File.Exists(fileName))
        {
            var str = File.ReadAllText(fileName);
            var existList = JsonSerializer.Deserialize<List<WishData>>(str);
            list.AddRange(existList.OrderBy(x => x.Id));
        }

        var json = JsonSerializer.Serialize(list.Distinct().OrderBy(x => x.Id), Const.JsonOptions);
        File.WriteAllText(fileName, json);
    }
}