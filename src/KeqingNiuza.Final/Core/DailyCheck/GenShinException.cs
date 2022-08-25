using System;

namespace KeqingNiuza.Core.DailyCheck;

public class GenShinException : Exception
{
    public GenShinException(string message) : base(message)
    {
    }
}