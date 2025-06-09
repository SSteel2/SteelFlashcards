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

        private readonly ObservableCollection<Tag> _tags;
        public ObservableCollection<Tag> Tags { get { return _tags; } }

        public LearnSelectionViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _tags = [];
            InitializeDummyData();
        }

        [RelayCommand]
        private void AcceptNavigate()
        {
            _navigationService.NavigateTo(nameof(LearnPage));
        }

        private void InitializeDummyData()
        {
            Tags.Add(new Tag("Tag 1", 10));
            Tags.Add(new Tag("Tag 15", 15));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("fdsafdas long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("Happpppppppfadsfsadppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("fasfasfa long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("trewtarsdfg long tag", 1));
            Tags.Add(new Tag("Happpppppppppppppppppppppppy long tag", 1));
            Tags.Add(new Tag("yolo", 1520));
        }

    }

    public class Tag(string tagName, int wordCount)
    {
        public string TagName { get; set; } = tagName;
        public int WordCount { get; set; } = wordCount;
    }
}
