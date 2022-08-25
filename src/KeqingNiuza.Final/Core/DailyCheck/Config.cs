﻿namespace KeqingNiuza.Core.DailyCheck;

/// <summary>
///     常量设定
/// </summary>
public static class Config
{
    public static string Ua =>
        $"Mozilla/5.0 miHoYoBBS/{AppVersion}";

    public static string AcceptEncoding => "gzip, deflate";

    public static string AppVersion => "2.34.1";

    public static string ClientType => "5";

    /// <summary>
    ///     盐(AppVersion的md5得到)
    /// </summary>
    public static string Salt => "9nQiU3AV0rJSIBWgdynfoGMGKaklfbM7";

    /// <summary>
    ///     获取头部DS
    /// </summary>
    /// <returns></returns>
    public static string GetDs()
    {
        var time = SafeUtil.GetCurrentTimestamp();
        var stringRom = SafeUtil.GetRandString(6).ToLower();
        var stringAdd = $"salt={Salt}&t={time}&r={stringRom}";
        var stringMd5 = SafeUtil.UserMd5(stringAdd);
        return $"{time},{stringRom},{stringMd5}";
    }

    #region Referer

    /// <summary>
    ///     活动ID，可能有变动
    /// </summary>
    public static string ActId => "e202009291139501";

    private static string BaseUrl => "https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html";

    public static string RefererUrl =>
        BaseUrl + $"?bbs_auth_required=true&act_id={ActId}&utm_source=bbs&utm_medium=mys&utm_campaign=icon";

    #endregion

    #region API

    /// <summary>
    ///     获取账号信息
    /// </summary>
    public static string GetUserGameRolesByCookie => "binding/api/getUserGameRolesByCookie?";

    /// <summary>
    ///     获取签到信息
    /// </summary>
    public static string GetBbsSignRewardInfo => "event/bbs_sign_reward/info?";

    /// <summary>
    ///     开始签到
    /// </summary>
    public static string PostSignInfo => "event/bbs_sign_reward/sign";

    #endregion
}