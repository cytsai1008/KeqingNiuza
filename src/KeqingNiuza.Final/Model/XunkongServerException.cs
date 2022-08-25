using System;

namespace KeqingNiuza.Model;

internal class XunkongServerException : Exception
{
    public XunkongServerException(string message) : base(message)
    {
    }
}