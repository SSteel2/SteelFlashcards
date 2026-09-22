using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SteelFlashcards;

public enum WordMasteryLevel
{
    // Word answered correctly last 3+ attempts, and last answer is less than 90 days ago
    Mastered,
    // Word answered correctly last 3+ attempts, but last answer is more than 90 days ago
    Stale,
    // Word answered correctly last 2 attempts
    True_2,
    // Word answered correctly last attempt
    True_1,
    // Word never attempted
    Zero,
    // Word answered incorrectly last attempt
    False_1,
    // Word answered incorrectly last 2+ attempts
    False_2
}

public class WordStatistic(string word)
{
    public string Word = word;
    // TODO: Ensure that answers are added in chronological order so that no ordering is needed afterwards
    public List<Answer> Answers = [];
    // TODO: proper access modifiers, once I figure out the classes
    private bool? m_isMastered;

    // Raised when an answer is added and the word's mastery state may have changed.
    public event EventHandler? AnswerAdded;

    private int m_correctAttempts = 0;
    private int m_recentAttempts = 0;
    private int m_recentCorrectAttempts = 0;

    public void AddAnswer(Answer answer)
    {
        Answers.Add(answer);
        if (answer.IsCorrect)
            m_correctAttempts++;
        if (answer.AttemptDateTime > DateTimeOffset.Now.AddDays(-90))
        {
            m_recentAttempts++;
            if (answer.IsCorrect)
                m_recentCorrectAttempts++;
        }
        m_isMastered = null;

        AnswerAdded?.Invoke(this, EventArgs.Empty);
    }

    public bool IsMastered
    {
        get {
            if (m_isMastered == null)
                CalculateMastery();
            return m_isMastered.Value;
        }
    }

    // TODO: this is really similar to calculate mastery level
    [MemberNotNull(nameof(m_isMastered))]
    private void CalculateMastery()
    {
        if (Answers.Count < 3)
        {
            m_isMastered = false;
            return;
        }
        // TODO: check if dates are in correct order
        Answers.Sort((x, y) => x.AttemptDateTime.CompareTo(y.AttemptDateTime));

        // TODO: check if last answer is less than 90 days ago
        m_isMastered = Answers[^1].IsCorrect && Answers[^2].IsCorrect && Answers[^3].IsCorrect;
    }

    public DateTimeOffset LastAttempt
    { 
        get {
            if (Answers.Count == 0)
                return DateTimeOffset.MinValue;
            Answers.Sort((x, y) => x.AttemptDateTime.CompareTo(y.AttemptDateTime));
            return Answers[^1].AttemptDateTime;
        }
    }

    public WordMasteryLevel CalculateMasteryLevel()
    {
        if (Answers.Count == 0)
            return WordMasteryLevel.Zero;
        Answers.Sort((x, y) => x.AttemptDateTime.CompareTo(y.AttemptDateTime));

        // TODO: check ordering
        bool streakValue = Answers[^1].IsCorrect;
        int streakCount = 1;
        if (Answers.Count > 1 && Answers[^2].IsCorrect == streakValue)
        {
            streakCount++;
            if (Answers.Count > 2 && Answers[^3].IsCorrect == streakValue)
                streakCount++;
        }

        if (!streakValue)
        {
            if (streakCount == 1)
                return WordMasteryLevel.False_1;
            else
                return WordMasteryLevel.False_2;

        }
        else
        {
            if (streakCount == 1)
                return WordMasteryLevel.True_1;
            else if (streakCount == 2)
                return WordMasteryLevel.True_2;
            else
            {
                if (Answers[^1].AttemptDateTime < DateTimeOffset.Now.AddDays(-90))
                    return WordMasteryLevel.Stale;
                else
                    return WordMasteryLevel.Mastered;
            }
        }
    }

    public string TotalAttemptsString()
    {
        return Statistics.GetFractionString(m_correctAttempts, Answers.Count);
    }

    public string RecentAttemptsString()
    {
        return Statistics.GetFractionString(m_recentCorrectAttempts, m_recentAttempts);
    }
}

public partial class TagStatistic(string tagName) : IDisposable
{
    public string TagName = tagName;
    public List<WordStatistic> Words = [];

    private int? m_wordsMastered;
    private bool m_disposed;

    public int WordsMastered
    {
        get
        {
            m_wordsMastered ??= CountMasteredWords();
            return m_wordsMastered.Value;
        }
    }

    private int CountMasteredWords()
    {
        int wordsMastered = 0;
        foreach (WordStatistic word in Words)
        {
            if (word.IsMastered)
                wordsMastered++;
        }
        return wordsMastered;
    }

    public void LinkWordStatistic(WordStatistic wordStatistic)
    {
        Words.Add(wordStatistic);
        wordStatistic.AnswerAdded += OnWordAnswerAdded;
    }

    private void OnWordAnswerAdded(object? sender, EventArgs e)
    {
        m_wordsMastered = null;
    }

    public bool IsMastered { get { return Words.Count == WordsMastered; } }

    public string GetWordsMasteryString()
    {
        return "Mastery: " + Statistics.GetFractionString(WordsMastered, Words.Count);
    }

    public void Dispose()
    {
        if (m_disposed)
            return;

        foreach (WordStatistic word in Words)
            word.AnswerAdded -= OnWordAnswerAdded;

        m_disposed = true;
        GC.SuppressFinalize(this);
    }
}

// View model class representing statistics calculated on the fly
// TODO: All 3 statistics classes should be seperated from ViewModel logic
public partial class Statistics : IDisposable
{
    int tagsMastered = 0;
    int tagsTotal = 0;
    int wordsMastered = 0;
    int wordsTotal = 0;
    public Dictionary<string, TagStatistic> tags = [];
    private readonly Dictionary<string, WordStatistic> words = [];

    // AddWord
    public void AddWord(WordEntry word)
    {
        WordStatistic wordStatistic = new(word.Word);
        // TODO: What happens if word already exists. For simplicity and not caring about degenerate cases right now lets ignore duplicates
        if (words.ContainsKey(word.Word))
        {
            // Maybe some warning for leter
            return;
        }
        
        words.Add(word.Word, wordStatistic);
        wordsTotal++;
        foreach (var tag in word.Tags)
        {
            if (!tags.TryGetValue(tag, out TagStatistic? tagStatistic))
            {
                tagStatistic = new TagStatistic(tag);
                tags.Add(tag, tagStatistic);
                tagsTotal++;
            }

            tagStatistic.LinkWordStatistic(wordStatistic);
        }
    }

    // AddAnswer
    public void AddAnswer(Answer answer)
    {
        // There might be deleted words in word entries with previous Answers
        if (!words.TryGetValue(answer.Word, out WordStatistic? value))
            return;
        value.AddAnswer(answer);
    }

    // Calculates mastery when all words are added
    public void CalculateMastery()
    {
        wordsMastered = 0;
        foreach (var word in words)
        {
            if (word.Value.IsMastered)
                wordsMastered++;
        }
        tagsMastered = 0;
        foreach (var tag in tags)
        {
            if (tag.Value.IsMastered)
                tagsMastered++;
        }
    }

    public WordMasteryLevel CalculateWordMasteryLevel(WordEntry word)
    {
        return words[word.Word].CalculateMasteryLevel();
    }

    // TODO: Move next 3 methods to StatisticsViewModel

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

    public void Dispose()
    {
        foreach (var tag in tags.Values)
            tag.Dispose();
        tags.Clear();
        GC.SuppressFinalize(this);
    }
}

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
