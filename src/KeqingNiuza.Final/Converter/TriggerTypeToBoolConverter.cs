using System;
using System.Globalization;
using System.Windows.Data;
using KeqingNiuza.Model;

namespace KeqingNiuza.Converter;

internal class TriggerTypeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = (ScheduleInfoTriggerType) value;
        if (type.ToString() == parameter.ToString())
            return true;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool) value)
        {
            if (Enum.TryParse<ScheduleInfoTriggerType>(parameter.ToString(), out var type))
                return type;
            return ScheduleInfoTriggerType.Countdown;
        }

        return ScheduleInfoTriggerType.None;
    }
}