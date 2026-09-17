using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SteelFlashcards;

public partial class StatisticsTagViewModel : ObservableObject
{
    private IDataService _dataService;
    private INavigationService _navigationService;

    [ObservableProperty]
    private TagStatistic? selectedTag;

    public StatisticsTagViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
    }

    public void InitializeTagStatistic(string? tagName)
    {
        if (tagName == null)
            throw new ApplicationException("Dev: TagName is null in InitializeTagStatistic");
        var selectedTag = _dataService.GetTagStatistic(tagName);
        if (selectedTag == null)
            throw new ApplicationException("Dev: _dataService.GetTagStatistic(TagName) returned null in InitializeTagStatistic");
        SelectedTag = selectedTag;
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
