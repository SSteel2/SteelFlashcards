using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LanguageLearn2
{
    public class WordStatistic(string word)
    {
        string word = word;
        List<Answer> answers = [];
        // TODO: proper access modifiers, once I figure out the classes
        public bool isMastered;

        public void AddAnswer(Answer answer)
        {
            answers.Add(answer);
        }

        public bool CalculateMastery()
        {
            if (answers.Count < 3)
            {
                isMastered = false;
                return false;
            }
            // TODO: check if dates are in correct order
            answers.Sort((x, y) => x.AttemptDateTime.CompareTo(y.AttemptDateTime));

            // TODO: check if last answer is less than 90 days ago
            isMastered = answers[^1].IsCorrect && answers[^2].IsCorrect && answers[^3].IsCorrect;
            return isMastered;
        }
    }

    public class TagStatistic(string tagName)
    {
        public string tagName = tagName;
        int wordsMastered = 0;
        int wordsTotal = 0;
        List<WordStatistic> words = [];

        public void LinkWordStatistic(WordStatistic wordStatistic)
        {
            words.Add(wordStatistic);
            wordsTotal++;
        }

        public bool CalculateMastery()
        {
            foreach (WordStatistic word in words)
            {
                if (word.isMastered)
                    wordsMastered++;
            }
            return wordsTotal == wordsMastered;
        }

        public string GetWordsMasteryString()
        {
            return "Mastery: " + Statistics.GetFractionString(wordsMastered, wordsTotal);
        }
    }

    public class Statistics
    {
        int tagsMastered = 0;
        int tagsTotal = 0;
        int wordsMastered = 0;
        int wordsTotal = 0;
        public Dictionary<string, TagStatistic> tags = [];
        Dictionary<string, WordStatistic> words = [];

        // AddWord
        public void AddWord(WordEntry word)
        {
            WordStatistic wordStatistic = new WordStatistic(word.Word);
            words.Add(word.Word, wordStatistic);
            wordsTotal++;
            foreach (var tag in word.Tags)
            {
                if (!tags.ContainsKey(tag))
                {
                    tags.Add(tag, new TagStatistic(tag));
                    tagsTotal++;
                }
                tags[tag].LinkWordStatistic(wordStatistic);
            }
        }

        // AddAnswer
        public void AddAnswer(Answer answer)
        {
            // There might be deleted words in word entries with previous answers
            if (!words.ContainsKey(answer.Word))
                return;

            words[answer.Word].AddAnswer(answer);
        }

        // Calculates mastery when all words are added
        public void CalculateMastery()
        {
            foreach (var word in words)
            {
                if (word.Value.CalculateMastery())
                    wordsMastered++;
            }
            foreach (var tag in tags)
            {
                if (tag.Value.CalculateMastery())
                    tagsMastered++;
            }
        }

        public string GetTagsMasteryString()
        {
            return "Tags " + GetFractionString(tagsMastered, tagsTotal);
        }

        public string GetWordsMasteryString()
        {
            return "Words " + GetFractionString(wordsMastered, wordsTotal);
        }

        public static string GetFractionString(int completed, int total)
        {
            return completed.ToString() + " / " + total.ToString();
        }
    }

    public partial class StatisticsViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        private Statistics m_statistics = new();

        [ObservableProperty]
        private string loadedDictionaryName;
        [ObservableProperty]
        private string masteredTagsString;
        [ObservableProperty]
        private string masteredWordsString;
        [ObservableProperty]
        private ObservableCollection<TagStatistic> tagStatistics = [];

        public StatisticsViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            InitializeDummyData();
            var loadedDictionary = _dataService.GetLoadedDictionary();
            LoadedDictionaryName = loadedDictionary == null ? "[No Dcitionary Loaded]" : loadedDictionary.DictionaryName;
            MasteredTagsString = m_statistics.GetTagsMasteryString();
            MasteredWordsString = m_statistics.GetWordsMasteryString();
            foreach (var tag in m_statistics.tags)
            {
                TagStatistics.Add(tag.Value);
            }
        }

        private void InitializeDummyData()
        {
            //m_statistics = new Statistics();
            m_statistics.AddWord(new WordEntry("a", ["a"], ["aaa"]));
            m_statistics.AddWord(new WordEntry("aa", ["a"], ["aaa"]));
            m_statistics.AddWord(new WordEntry("b", ["b"], ["bbb"]));
            m_statistics.AddWord(new WordEntry("bb", ["b"], ["bbb"]));
            m_statistics.AddWord(new WordEntry("ab", ["ab"], ["bbb", "aaa"]));
            m_statistics.AddWord(new WordEntry("c", ["c"], ["ccc"]));
            m_statistics.AddWord(new WordEntry("cc", ["c"], ["ccc"]));
            m_statistics.AddWord(new WordEntry("ccc", ["c"], ["ccc"]));
            m_statistics.AddAnswer(new Answer { Word = "a", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "a", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "a", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "aa", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "aa", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "aa", Guess = "a", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "b", Guess = "b", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "b", Guess = "b", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "b", Guess = "b", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "bb", Guess = "b", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "bb", Guess = "b", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "ab", Guess = "ab", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "ab", Guess = "abb", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "ab", Guess = "abb", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "c", Guess = "d", IsCorrect = false, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "c", Guess = "c", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "c", Guess = "c", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "cc", Guess = "c", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "cc", Guess = "c", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "cc", Guess = "c", IsCorrect = true, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "cc", Guess = "d", IsCorrect = false, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.AddAnswer(new Answer { Word = "cc", Guess = "d", IsCorrect = false, AttemptDateTime = System.DateTimeOffset.Now });
            m_statistics.CalculateMastery();
        }
    }
}
