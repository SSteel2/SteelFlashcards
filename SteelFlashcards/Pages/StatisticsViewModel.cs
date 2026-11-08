using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SteelFlashcards;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    private Statistics m_statistics;

    [ObservableProperty] public partial string LoadedDictionaryName { get; set; } = string.Empty;
    [ObservableProperty] public partial string MasteredTagsString { get; set; } = string.Empty;
    [ObservableProperty] public partial string MasteredWordsString { get; set; } = string.Empty;
    [ObservableProperty] public partial ObservableCollection<TagStatistic> TagStatistics { get; set; } = new ObservableCollection<TagStatistic>();
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ViewTagCommand))] public partial TagStatistic? SelectedTag { get; set; }

    public StatisticsViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
        var loadedDictionary = _dataService.GetLoadedDictionary();
        LoadedDictionaryName = loadedDictionary == null ? "[No Dictionary Loaded]" : loadedDictionary.DictionaryName;
        LoadStatistics();
    }

    [RelayCommand]
    [MemberNotNull(nameof(m_statistics))]
    public void LoadStatistics()
    {
        m_statistics = _dataService.GetStatistics();
        m_statistics.CalculateMastery();
        MasteredTagsString = m_statistics.GetTagsMasteryString();
        MasteredWordsString = m_statistics.GetWordsMasteryString();
        foreach (var tag in m_statistics.tags)
        {
            TagStatistics.Add(tag.Value);
        }
    }

    [RelayCommand(CanExecute = nameof(IsTagSelected))]
    private void ViewTag()
    {
        if (SelectedTag == null)
            throw new ApplicationException("Dev Error: SelectedTag is null in ViewTag");
        _navigationService.NavigateTo(nameof(StatisticsTagPage), SelectedTag.TagName);
    }

    private bool IsTagSelected()
    {
        return SelectedTag != null;
    }
}
