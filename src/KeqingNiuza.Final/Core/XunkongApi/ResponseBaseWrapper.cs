namespace KeqingNiuza.Core.XunkongApi;

internal class ResponseBaseWrapper<TData>
{
    public int Code { get; set; }

    public string Message { get; set; }

    public TData Data { get; set; }
}