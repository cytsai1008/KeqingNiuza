using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using KeqingNiuza.Model;

namespace KeqingNiuza.Converter;

internal class TriggerTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = (ScheduleInfoTriggerType) value;
        if (parameter.ToString().Contains(type.ToString()))
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Enum.TryParse<ScheduleInfoTriggerType>(parameter.ToString(), out var type))
            return type;
        return ScheduleInfoTriggerType.None;
    }
}