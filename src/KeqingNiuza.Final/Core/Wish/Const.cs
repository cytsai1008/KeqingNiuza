﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace KeqingNiuza.Core.Wish;

public static class Const
{
    public static readonly List<string> BrushList = new()
    {
        "#FF0000",
        "#FF1493",
        "#FF7F50",
        "#FFB61E",
        "#8C531B",
        "#a88462",
        "#008B8B",
        "#228B22",
        "#789262",
        "#7BBFEA",
        "#4169E1",
        "#0000FF",
        "#B0A4E3",
        "#800080"
    };

    public static List<WishEvent> WishEventList => LoadWishEventList();
    public static List<CharacterInfo> CharacterInfoList => LoadCharacterInfoList();
    public static List<WeaponInfo> WeaponInfoList => LoadWeaponInfoList();

    public static List<WishEvent> LoadWishEventList()
    {
        if (File.Exists("Resource\\List\\WishEventList.json"))
        {
            var json = File.ReadAllText("Resource\\List\\WishEventList.json");
            return JsonSerializer.Deserialize<List<WishEvent>>(json);
        }

        return new List<WishEvent>();
    }

    public static List<CharacterInfo> LoadCharacterInfoList()
    {
        if (File.Exists("Resource\\List\\CharacterInfoList.json"))
        {
            var json = File.ReadAllText("Resource\\List\\CharacterInfoList.json");
            return JsonSerializer.Deserialize<List<CharacterInfo>>(json);
        }

        return new List<CharacterInfo>();
    }

    public static List<WeaponInfo> LoadWeaponInfoList()
    {
        if (File.Exists("Resource\\List\\WeaponInfoList.json"))
        {
            var json = File.ReadAllText("Resource\\List\\WeaponInfoList.json");
            return JsonSerializer.Deserialize<List<WeaponInfo>>(json);
        }

        return new List<WeaponInfo>();
    }
}