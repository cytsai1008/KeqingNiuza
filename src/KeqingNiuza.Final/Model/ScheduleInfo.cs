using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeqingNiuza.Model;

public class ScheduleInfo : INotifyPropertyChanged
{
    private int _Custom_DelayDay;


    private DateTime _Custom_TriggerTime;


    private TimeSpan _Interval;


    private bool _IsEnable = true;


    private DateTime _LastTriggerTime;


    private DateTime _LastZeroValueTime;


    private int _MaxValue;

    private string _Name;


    private TimeSpan _TimePerValue;


    private ScheduleInfoTriggerType _TriggerType;

    public string Name
    {
        get => _Name;
        set
        {
            _Name = value;
            OnPropertyChanged();
        }
    }

    public bool IsEnable
    {
        get => _IsEnable;
        set
        {
            _IsEnable = value;
            OnPropertyChanged();
        }
    }

    public ScheduleInfoTriggerType TriggerType
    {
        get => _TriggerType;
        set
        {
            if (value == ScheduleInfoTriggerType.None) return;
            _TriggerType = value;
            OnPropertyChanged();
        }
    }

    public DateTime LastTriggerTime
    {
        get => _LastTriggerTime;
        set
        {
            _LastTriggerTime = value;
            OnPropertyChanged();
        }
    }

    [JsonConverter(typeof(IntervalJsonConverter))]
    public TimeSpan Interval
    {
        get => _Interval;
        set
        {
            _Interval = value;
            OnPropertyChanged();
        }
    }


    [JsonIgnore]
    public DateTime NextTriggerTime
    {
        get => LastTriggerTime + Interval;
        set
        {
            LastTriggerTime = value - Interval;
            OnPropertyChanged();
        }
    }


    [JsonIgnore]
    public TimeSpan RemainingTime
    {
        get
        {
            var span = NextTriggerTime - DateTime.Now;
            if (span < new TimeSpan())
                return new TimeSpan();
            return span;
        }
        set
        {
            LastTriggerTime = DateTime.Now + value - Interval;
            OnPropertyChanged();
        }
    }

    public int MaxValue
    {
        get => _MaxValue;
        set
        {
            _MaxValue = value;
            OnPropertyChanged();
        }
    }

    [JsonConverter(typeof(IntervalJsonConverter))]
    public TimeSpan TimePerValue
    {
        get => _TimePerValue;
        set
        {
            _TimePerValue = value;
            OnPropertyChanged();
        }
    }

    public DateTime LastZeroValueTime
    {
        get => _LastZeroValueTime;
        set
        {
            _LastZeroValueTime = value;
            OnPropertyChanged();
        }
    }


    [JsonIgnore]
    public int CurrentValue
    {
        get
        {
            var current = (int) ((DateTime.Now - LastZeroValueTime).TotalSeconds / TimePerValue.TotalSeconds);
            return current > MaxValue ? MaxValue : current < 0 ? 0 : current;
        }
        set
        {
            // 值限制在最大最小范围内
            var current = value > MaxValue ? MaxValue : value < 0 ? 0 : value;
            LastZeroValueTime = DateTime.Now - new TimeSpan(TimePerValue.Ticks * current);
            OnPropertyChanged();
        }
    }


    [JsonIgnore]
    public DateTime NextMaxValueTime
    {
        get => LastZeroValueTime + new TimeSpan(TimePerValue.Ticks * MaxValue);
        set
        {
            LastZeroValueTime = value - new TimeSpan(TimePerValue.Ticks * MaxValue);
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     下一次提醒时间在几天后
    /// </summary>
    public int Custom_DelayDay
    {
        get => _Custom_DelayDay;
        set
        {
            _Custom_DelayDay = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     提醒时在当天的几点
    /// </summary>
    public DateTime Custom_TriggerTime
    {
        get => _Custom_TriggerTime;
        set
        {
            _Custom_TriggerTime = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public void Refresh()
    {
        OnPropertyChanged("RemainingTime");
        OnPropertyChanged("CurrentValue");
    }
}

internal class IntervalJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var ticks = reader.GetInt64();
        return new TimeSpan(ticks);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Ticks);
    }
}