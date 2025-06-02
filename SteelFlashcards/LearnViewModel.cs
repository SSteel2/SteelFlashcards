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

        private WordEntry? m_currentWordEntry;

        private IList<WordEntry> m_words;

        private Random m_randomGenerator;

        private readonly ObservableCollection<Answer> answers = [];

        public ObservableCollection<Answer> Answers { get { return answers; } }

        public LearnViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            m_words = _dataService.GetWords();
            m_randomGenerator = new Random();
            SetNextWord();
        }

        [RelayCommand]
        public void AcceptAnswer(string guess)
        {
            // Should never be null, will be removed once design changes
            if (m_currentWordEntry == null)
                return;

            var answer = _dataService.AddAnswer(guess, m_currentWordEntry);
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
            int nextIndex = m_randomGenerator.Next(0, m_words.Count);
            m_currentWordEntry = m_words[nextIndex];
            CurrentWord = m_currentWordEntry.Word;
        }

    }
}
