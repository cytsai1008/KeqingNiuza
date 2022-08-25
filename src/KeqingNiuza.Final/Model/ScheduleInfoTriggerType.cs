namespace KeqingNiuza.Model;

public enum ScheduleInfoTriggerType
{
    None,

    /// <summary>
    ///     倒计时
    /// </summary>
    Countdown,

    /// <summary>
    ///     恢复
    /// </summary>
    Recovery,

    /// <summary>
    ///     固定时间
    /// </summary>
    FixedTime
}