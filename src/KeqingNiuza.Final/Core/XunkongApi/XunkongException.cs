using System;

namespace KeqingNiuza.Core.XunkongApi;

internal class XunkongException : Exception
{
    public XunkongException()
    {
    }


    public XunkongException(int code, string message = null) : base(message)
    {
        Code = code;
    }

    public int Code { get; }
}