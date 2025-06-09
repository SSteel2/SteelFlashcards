using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearn2
{
    public partial class LearnSelectionViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

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

        [RelayCommand]
        private void AcceptNavigate()
        {
            _dataService.SetActiveTags(SelectedTags);
            _navigationService.NavigateTo(nameof(LearnPage));
        }

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
        }
    }
}
