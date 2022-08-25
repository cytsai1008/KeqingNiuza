using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace KeqingNiuza.Core.DailyCheck;

/// <summary>
///     创建Json实体到Post的JsonBody
/// </summary>
public class JsonContent : ByteArrayContent
{
    public JsonContent(object data) : base(ToBytes(data))
    {
        Data = data;
        Headers.ContentType = new MediaTypeHeaderValue("application/json");
    }

    private object Data { get; }

    private static byte[] ToBytes(object data)
    {
        var rawData = JsonConvert.SerializeObject(data);

        return Encoding.UTF8.GetBytes(rawData);
    }

    public JsonContent Clone()
    {
        return new JsonContent(Data);
    }
}