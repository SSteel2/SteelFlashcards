using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LanguageLearn2
{
    public partial class LearnViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        [ObservableProperty]
        private string? currentWord;

        private WordEntry? m_currentWordEntry;

        private readonly IList<WordEntry> m_words;

        private readonly Random m_randomGenerator;

        private readonly ObservableCollection<Answer> answers = [];

        public ObservableCollection<Answer> Answers { get { return answers; } }

        [ObservableProperty]
        private Answer? lastAnswer;

        public LearnViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            m_words = _dataService.GetWords();
            m_randomGenerator = new Random();
            LastAnswer = null;
            SetNextWord();
        }

        [RelayCommand]
        public void AcceptAnswer(string guess)
        {
            // Should never be null, will be removed once design changes
            if (m_currentWordEntry == null)
                return;

            Answer answer = CreateAnswer(guess);
            answers.Add(answer);
            _dataService.AddAnswer(answer);
            LastAnswer = answer;
            SetNextWord();
        }

        [RelayCommand]
        public void NewAnswers()
        {
            _dataService.ClearAnswers();
            answers.Clear();
            Answer.Reset();
            LastAnswer = null;
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

        private Answer CreateAnswer(string guess)
        {
            if (m_currentWordEntry == null)
                throw new ApplicationException("Current word entry is null in CreateAnswer");

            string sanitizedGuess = guess.Trim();
            int minDistance = int.MaxValue;
            string bestFittingMeaning = string.Empty;
            foreach (string meaning in m_currentWordEntry.Meanings)
            {
                string sanitizedMeaning = SanitizeMeaning(meaning);
                int levensteinDistance = LevenshteinDistance(sanitizedGuess, sanitizedMeaning);
                if (levensteinDistance < minDistance)
                {
                    minDistance = levensteinDistance;
                    bestFittingMeaning = meaning;
                }
            }
            
            return new Answer(m_currentWordEntry.Word, bestFittingMeaning, guess, minDistance <= 1);
        }

        private static string SanitizeMeaning(string meaning)
        {
            string trimmedMeaning = meaning;
            int openParanthesisIndex = meaning.IndexOf('(');
            if (openParanthesisIndex > -1)
            {
                trimmedMeaning = meaning[..openParanthesisIndex];
            }
            return trimmedMeaning.Trim().ToLower();
        }

        private static int LevenshteinDistance(string word1, string word2)
        {
            int[,] distance = new int[word1.Length + 1, word2.Length + 1];
            for (int i = 0; i <= word1.Length; i++)
                distance[i, 0] = i;
            for (int i = 1; i <= word2.Length; i++)
                distance[0, i] = i;
            
            for (int i = 1; i <= word1.Length; i++)
            {
                for (int j = 1; j <= word2.Length; j++)
                {
                    // TODO: Common mistypes like 1 -> a_nosine, or a -> a_nosine could be detected in here as well
                    int substitutionCost = word1[i - 1] == word2[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(distance[i - 1, j] + 1, Math.Min(distance[i, j - 1] + 1, distance[i - 1, j - 1] + substitutionCost));
                }
            }
            return distance[word1.Length, word2.Length];
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "\u2714" : "\u274C";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
