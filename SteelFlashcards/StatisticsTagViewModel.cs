using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace LanguageLearn2
{
    public partial class StatisticsTagViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        [ObservableProperty]
        private TagStatistic selectedTag;
        //[ObservableProperty]
        //private string masteredWordsString;

        public StatisticsTagViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }

        public void InitializeTagStatistic(string? tagName)
        {
            if (tagName == null)
                throw new ApplicationException("Dev: tagName is null in InitializeTagStatistic");
            SelectedTag = _dataService.GetTagStatistic(tagName);
            if (SelectedTag == null)
                throw new ApplicationException("Dev: _dataService.GetTagStatistic(tagName) returned null in InitializeTagStatistic");

            //MasteredWordsString = SelectedTag.GetWordsMasteryString();
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
