using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeqingNiuza.Core.Wish;

public class AchievementAnalyzer
{
    private readonly List<WishData> _WishDataList;

    public AchievementAnalyzer(string path)
    {
        _WishDataList = LocalWishLogLoader.Load(path);
        Analyzer();
    }

    public AchievementAnalyzer(List<WishData> list)
    {
        _WishDataList = list;
        Analyzer();
    }

    public List<AchievementInfo> AchievementList { get; set; }


    public void Analyzer()
    {
        var tempList = new List<AchievementInfo>();
        var methods = typeof(AchievementComputeMethod).GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (var method in methods) method.Invoke(null, new object[] {_WishDataList, tempList});
        AchievementList = tempList.OrderByDescending(x => x.IsFinished).ThenByDescending(x => x.FinishTime).ToList();
    }
}