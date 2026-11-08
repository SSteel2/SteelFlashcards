using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SteelFlashcards;

public partial class LearnSelectionViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    private readonly ObservableCollection<DictionaryTag> _tags;
    public ObservableCollection<DictionaryTag> Tags { get { return _tags; } }

    public List<DictionaryTag> SelectedTags { get; } = [];

    public LearnSelectionViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
        _tags = [];
        InitializeTags();
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void AcceptNavigate()
    {
        if (SelectedTags.Count == 0)
        {
            _dataService.SetActiveTags(_tags);
        }
        else
        {
            _dataService.SetActiveTags(SelectedTags);
        }
        _navigationService.NavigateTo(nameof(LearnPage));
    }

    private bool CanStart => _tags.Any(tag => tag.WordCount > 0);

    private void InitializeTags()
    {
        DictionaryFile? loadedDictionary = _dataService.GetLoadedDictionary();
        if (loadedDictionary == null)
            return; // TODO: Add some proper UI, which indicates that no dictionary is loaded
        var tags = loadedDictionary.GetTags();
        foreach (var tag in tags)
        {
            Tags.Add(tag);
        }
        AcceptNavigateCommand.NotifyCanExecuteChanged();
    }
}
