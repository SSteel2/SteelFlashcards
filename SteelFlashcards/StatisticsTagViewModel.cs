using CommunityToolkit.Mvvm.ComponentModel;

namespace LanguageLearn2
{
    public class StatisticsTagViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        public StatisticsTagViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }
    }
}
