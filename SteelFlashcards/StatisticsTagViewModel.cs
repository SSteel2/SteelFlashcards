using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace LanguageLearn2
{
    public class StatisticsTagViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        private TagStatistic selectedTag;

        public StatisticsTagViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }

        public void InitializeTagStatistic(string? tagName)
        {
            if (tagName == null)
                throw new ApplicationException("Dev: tagName is null in InitializeTagStatistic");
            selectedTag = _dataService.GetTagStatistic(tagName);
            if (selectedTag == null)
                throw new ApplicationException("Dev: _dataService.GetTagStatistic(tagName) returned null in InitializeTagStatistic");
        }
    }
}
