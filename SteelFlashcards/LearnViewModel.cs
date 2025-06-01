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

        private IList<WordEntry> m_words;

        private int m_wordIndex;

        private readonly ObservableCollection<Answer> answers = [];

        public ObservableCollection<Answer> Answers { get { return answers; } }

        public LearnViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            m_words = _dataService.GetWords();
            m_wordIndex = 0;
            SetNextWord();
        }

        [RelayCommand]
        public void AcceptAnswer(string guess)
        {
            var answer = _dataService.AddAnswer(guess);
            answers.Add(answer);
            SetNextWord();
        }

        [RelayCommand]
        public void NewAnswers()
        {
            _dataService.ClearAnswers();
            answers.Clear();
            Answer.Reset();
        }

        private void SetNextWord()
        {
            // TODO: jei nera zodziu tai cia isvis nereiktu patekti
            if (m_words.Count == 0)
            {
                CurrentWord = "[No Words in Dictionary]";
                return;
            }
            CurrentWord = m_words[m_wordIndex++].Word;
            if (m_wordIndex >= m_words.Count)
                m_wordIndex = 0;
        }

    }
}
