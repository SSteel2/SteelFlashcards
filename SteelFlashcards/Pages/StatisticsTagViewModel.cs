using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SteelFlashcards;

public partial class StatisticsTagViewModel(IDataService dataService, INavigationService navigationService) : ObservableObject
{
    private readonly IDataService _dataService = dataService;
    private readonly INavigationService _navigationService = navigationService;

    [ObservableProperty]
    public partial TagStatistic? SelectedTag { get; set; }

    public void InitializeTagStatistic(string? tagName)
    {
        if (tagName == null)
            throw new ApplicationException("Dev: TagName is null in InitializeTagStatistic");
        SelectedTag = _dataService.GetTagStatistic(tagName) ?? throw new ApplicationException("Dev: _dataService.GetTagStatistic(TagName) returned null in InitializeTagStatistic");
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
