using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KeqingNiuza.Core.Wish;
using KeqingNiuza.Model;

namespace KeqingNiuza.ViewModel;

public class WishAchievementViewModel : INotifyPropertyChanged
{
    private List<AchievementInfo> _AchievementInfoList;


    public WishAchievementViewModel(UserData userData)
    {
        var analyzer = new AchievementAnalyzer(userData.WishLogFile);
        AchievementInfoList = analyzer.AchievementList;
    }


    public WishAchievementViewModel()
    {
        var analyzer = new AchievementAnalyzer(MainWindowViewModel.WishDataList);
        AchievementInfoList = analyzer.AchievementList;
    }

    public List<AchievementInfo> AchievementInfoList
    {
        get => _AchievementInfoList;
        set
        {
            _AchievementInfoList = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}