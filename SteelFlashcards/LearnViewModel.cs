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
    public partial class LearnViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        [ObservableProperty]
        private string? currentWord;

        private readonly ObservableCollection<Answer> answers = [];

        public ObservableCollection<Answer> Answers { get { return answers; } }

        public LearnViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            CurrentWord = _dataService.GetNextWord().Word;
        }

        [RelayCommand]
        public void AcceptAnswer(string guess)
        {
            var answer = _dataService.AddAnswer(guess);
            answers.Add(answer);
            CurrentWord = _dataService.GetNextWord().Word;
        }

        [RelayCommand]
        public void NewAnswers()
        {
            _dataService.ClearAnswers();
            answers.Clear();
            Answer.Reset();
        }

    }
}
