using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace KeqingNiuza.Core.CloudBackup;

internal class Const
{
    public static JsonSerializerOptions JsonOptions = new()
        {Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), WriteIndented = true};

    public static string UserDataPath { get; } = "..\\UserData";
}