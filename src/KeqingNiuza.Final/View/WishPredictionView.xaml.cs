﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeqingNiuza.Core.Wish;
using LiveCharts;
using LiveCharts.Wpf;

namespace KeqingNiuza.View;

/// <summary>
///     WishPredictionView.xaml 的交互逻辑
/// </summary>
public partial class WishPredictionView : UserControl, INotifyPropertyChanged
{
    private Visibility _AxisSectionVisibility = Visibility.Hidden;


    private bool _ShowSectionDataLabel;


    private ChartValues<double> _Value;

    private int _XSection;

    private double _YSection;


    public WishPredictionView()
    {
        InitializeComponent();
    }

    public ChartValues<double> Value
    {
        get => _Value;
        set
        {
            _Value = value;
            OnPropertyChanged();
        }
    }

    public int XSection
    {
        get => _XSection;
        set
        {
            _XSection = value;
            OnPropertyChanged();
        }
    }

    public double YSection
    {
        get => _YSection;
        set
        {
            _YSection = value;
            OnPropertyChanged();
        }
    }

    public Visibility AxisSectionVisibility
    {
        get => _AxisSectionVisibility;
        set
        {
            _AxisSectionVisibility = value;
            OnPropertyChanged();
        }
    }

    public bool ShowSectionDataLabel
    {
        get => _ShowSectionDataLabel;
        set
        {
            _ShowSectionDataLabel = value;
            OnPropertyChanged();
        }
    }


    public Func<double, string> YSectionFormatter => value => value.ToString("P4");
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var count = (int) NumericUpDown.Value;
        var index = ComboBox.SelectedIndex;
        await Task.Run(() =>
        {
            switch (index)
            {
                case 0:
                    Value = new ChartValues<double>(Prediction.GetCharacterDensityAndDistributionWithUp(count)
                        .distribution.Prepend(0));
                    break;
                case 1:
                    Value = new ChartValues<double>(Prediction.GetCharacterDensityAndDistribution(count).distribution
                        .Prepend(0));
                    break;
                case 2:
                    Value = new ChartValues<double>(Prediction.GetSpecifiedWeaponDensityAndDistribution(count)
                        .distribution.Prepend(0));
                    break;
                case 3:
                    Value = new ChartValues<double>(Prediction.GetWeaponDensityAndDistribution(count).distribution
                        .Prepend(0));
                    break;
            }
        });
    }


    private void CartesianChart_MouseMove(object sender, MouseEventArgs e)
    {
        if (Value != null)
        {
            var chart = sender as CartesianChart;
            var point = e.GetPosition(chart);
            var p = chart.ConvertToChartValues(point);
            XSection = (int) Math.Round(p.X);
            try
            {
                YSection = Value[XSection];
            }
            catch
            {
            }
        }
    }

    private void CartesianChart_MouseEnter(object sender, MouseEventArgs e)
    {
        AxisSectionVisibility = Visibility.Visible;
        ShowSectionDataLabel = true;
    }

    private void CartesianChart_MouseLeave(object sender, MouseEventArgs e)
    {
        AxisSectionVisibility = Visibility.Hidden;
        ShowSectionDataLabel = false;
    }
}