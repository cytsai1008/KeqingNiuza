using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeqingNiuza.Core.Wish;
using KeqingNiuza.Model;
using Newtonsoft.Json.Linq;

namespace KeqingNiuza.Service;

internal class WishlogBackupService
{
    private const string WishlogBackupUrl = "https://api.xk.scighost.com/v1/wishlog";
    private const string WishlogBackupUrl2 = "https://api.xunkong.cc/v0.1/wishlog";
    private const string WishlogBackupUrl3 = "https://localhost:44362/v0.1/wishlog";

    private readonly HttpClient _client;

    public WishlogBackupService()
    {
        _client = new HttpClient(new HttpClientHandler
            {AllowAutoRedirect = true, AutomaticDecompression = DecompressionMethods.GZip});
        _client.DefaultRequestHeaders.Add("User-Agent", $"KeqingNiuza/{Const.FileVersion}");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        _client.DefaultRequestHeaders.Add("X-Device-Id", Const.UserId);
        _client.Timeout = TimeSpan.FromSeconds(30);
    }

    public string RequestInfo { get; set; }


    public async Task EnsureAuthkeyAsync(int uid, string url)
    {
        if (uid == 0) throw new ArgumentException(nameof(uid));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("没有保存此Uid的祈愿记录网址，请加载数据后重试");
        var client = new WishLogExporter(url);
        var urlUid = await client.GetUidByUrl();
        if (uid != urlUid) throw new ArgumentException("祈愿记录网址和Uid不匹配");
    }


    public async Task<WishlogResult> GetServerDataAsync(int uid, string url)
    {
        var model = new WishlogModel
        {
            Uid = uid,
            Url = url
        };
        var message = await _client.PostAsJsonAsync(WishlogBackupUrl2 + "/" + "last", model);
        message.EnsureSuccessStatusCode();
        var response = await message.Content.ReadFromJsonAsync<ResponseData<WishlogResult>>(new JsonSerializerOptions
            {PropertyNameCaseInsensitive = true});
        return response.Data;
    }


    public async Task<WishlogResult> ExecuteAsync(int uid, string url, string operation, IEnumerable<WishData> list)
    {
        var model = new WishlogModel
        {
            Uid = uid,
            Url = url,
            List = list
        };
        var message = await _client.PostAsJsonAsync(WishlogBackupUrl2 + "/" + operation, model);
        var builder = new StringBuilder();
        builder.AppendLine(DateTime.Now.ToString("s"));
        builder.AppendLine("Request:");
        builder.AppendLine($"uid: {uid}");
        builder.AppendLine($"url: {url}");
        builder.AppendLine($"operation: {operation}");
        builder.AppendLine($"wishlogCount: {list?.Count() ?? 0}");
        builder.AppendLine("Response:");
        builder.AppendLine($"statusCode: {(int) message.StatusCode}");
        if (message.Content.Headers.TryGetValues("X-Fc-Request-Id", out var requestId))
            builder.AppendLine($"requestId: {string.Join(" ", requestId)}");
        try
        {
            if (message.IsSuccessStatusCode)
            {
                var response =
                    await message.Content.ReadFromJsonAsync<ResponseData<WishlogResult>>(new JsonSerializerOptions
                        {PropertyNameCaseInsensitive = true});
                builder.AppendLine($"code: {response.Code}");
                builder.AppendLine($"msg: {response.Message}");
                if (response.Code != 0)
                {
                    throw new XunkongServerException(response.Message);
                }

                builder.AppendLine($"uid: {response.Data?.Uid}");
                builder.AppendLine($"currentCount: {response.Data?.CurrentCount}");
                builder.AppendLine($"getCount: {response.Data?.GetCount}");
                builder.AppendLine($"putCount: {response.Data?.PutCount}");
                builder.AppendLine($"deleteCount: {response.Data?.DeleteCount}");
                return response.Data;
            }

            if (message.StatusCode == HttpStatusCode.InternalServerError &&
                message.Content.Headers.ContentType.MediaType.Contains("json"))
            {
                var node = JObject.Parse(await message.Content.ReadAsStringAsync());
                foreach (var item in node) builder.AppendLine($"{item.Key}: {item.Value}");
                throw new HttpRequestException(builder.ToString());
            }

            message.EnsureSuccessStatusCode();
        }
        finally
        {
            var str = builder.ToString();
            Log.OutputLog(LogType.Info, str);
            RequestInfo = str;
        }

        return null;
    }
}