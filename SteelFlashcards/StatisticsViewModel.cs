using CommunityToolkit.Mvvm.ComponentModel;

namespace LanguageLearn2
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        public StatisticsViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }
    }
}
