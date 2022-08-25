using System;
using System.Windows;
using System.Windows.Controls;

namespace KeqingNiuza.View;

/// <summary>
///     ErrorView.xaml 的交互逻辑
/// </summary>
public partial class ErrorView : UserControl
{
    private readonly Exception Ex;

    public ErrorView(Exception ex)
    {
        InitializeComponent();
        TextBlock_ErrorMessage.Text = GetInnerMessage(ex);
        Ex = ex;
    }

    private void Button_Detail_Click(object sender, RoutedEventArgs e)
    {
        TextBlock_Detail.Text = Ex.ToString();
    }

    private string GetInnerMessage(Exception ex)
    {
        var mesage = ex.Message;
        var innerEx = ex;
        while (innerEx.InnerException != null)
        {
            innerEx = innerEx.InnerException;
            mesage = innerEx.Message;
        }

        return mesage;
    }
}