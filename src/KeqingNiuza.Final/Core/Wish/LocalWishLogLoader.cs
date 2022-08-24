﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;

namespace KeqingNiuza.Core.Wish
{
    public class LocalWishLogLoader
    {
        /// <summary>
        /// 从本地文件中加载祈愿记录，自动完成总次数和保底内的计算，以id升序排序
        /// </summary>
        /// <param name="file">文件地址</param>
        /// <returns></returns>
        public static List<WishData> Load(string file)
        {
            var WishEventList = Const.WishEventList;
            var json = File.ReadAllText(file);
            var originalList = JsonSerializer.Deserialize<List<WishData>>(json);
            if (originalList == null || originalList.Count == 0)
            {
                return null;
            }
            var resultList = new List<WishData>(originalList.Count);
            var groups = originalList.GroupBy(x => x.QueryType);
            foreach (var group in groups)
            {
                resultList.AddRange(ComputeWishInfo(WishEventList, group.ToList()));
            }

            return resultList.OrderBy(x => x.Id).ToList();
        }


        private static List<WishData> ComputeWishInfo(List<WishEvent> WishEventList, List<WishData> subList)
        {
            int tmp = -1;
            bool is_DaBaoDi = false;
            for (int i = 0; i < subList.Count(); i++)
            {
                var data = subList[i];
                data.Guarantee = i - tmp;
                data.Number = i;
                data.GuaranteeType = is_DaBaoDi ? "大保底" : "小保底";
                data.Name = ChineseConverter.Convert(data.Name, ChineseConversionDirection.TraditionalToSimplified);
                if (data.WishType == WishType.Permanent || data.WishType == WishType.Novice)
                {
                    data.GuaranteeType = "保底内";
                }
                if (data.Rank == 5)
                {
                    tmp = i;
                    if (data.WishType != WishType.Permanent && data.WishType != WishType.Novice)
                    {
                        var wishevent = WishEventList.Find(x =>
                            x.WishType == data.QueryType
                            && x.StartTime <= data.Time
                            && x.EndTime >= data.Time);
                        if (wishevent != null)
                        {
                            if (wishevent.Rank5UpItems.Contains(data.Name))
                            {
                                is_DaBaoDi = false;
                            }
                            else
                            {
                                is_DaBaoDi = true;
                            }
                        }
                    }
                }
                if (data.Rank == 5 || data.Rank == 4)
                {
                    if (data.WishType != WishType.Permanent && data.WishType != WishType.Novice)
                    {

                        var wishevent = WishEventList.Find(x =>
                            x.WishType == data.QueryType
                            && x.StartTime <= data.Time
                            && x.EndTime >= data.Time);
                        if (wishevent != null)
                        {
                            if (wishevent.Rank5UpItems.Contains(data.Name) || wishevent.Rank4UpItems.Contains(data.Name))
                            {
                                data.IsUp = true;
                            }
                            else
                            {
                                data.IsUp = false;
                            }
                        }
                    }
                }
            }

            return subList;
        }
    }
}
