using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SteelFlashcards;

public class WordStatistic(string word)
{
    public string Word = word;
    public List<Answer> Answers = [];
    // TODO: proper access modifiers, once I figure out the classes
    private bool? m_isMastered;

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
    }

    public bool IsMastered
    {
        get {
            if (m_isMastered == null)
                CalculateMastery();
            return m_isMastered.Value;
        }
    }

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

    public string TotalAttemptsString()
    {
        return Statistics.GetFractionString(m_correctAttempts, Answers.Count);
    }

    public string RecentAttemptsString()
    {
        return Statistics.GetFractionString(m_recentCorrectAttempts, m_recentAttempts);
    }
}

public class TagStatistic(string tagName)
{
    public string TagName = tagName;
    public List<WordStatistic> Words = [];
    
    private int? m_wordsMastered;

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
    }

    public bool IsMastered { get { return Words.Count == WordsMastered; } }

    public string GetWordsMasteryString()
    {
        return "Mastery: " + Statistics.GetFractionString(WordsMastered, Words.Count);
    }
}

// View model class representing statistics calculated on the fly
// TODO: All 3 statistics classes should be seperated from ViewModel logic
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
        // TODO: What happens if word already exists. For simplicity and not caring about degenerate cases right now
        // lets ignore duplicates
        if (words.ContainsKey(word.Word))
        {
            // Maybe some warning for leter
            return;
        }
        
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
        // There might be deleted words in word entries with previous Answers
        if (!words.ContainsKey(answer.Word))
            return;

        words[answer.Word].AddAnswer(answer);
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
}

public partial class StatisticsViewModel : ObservableObject
{
    private IDataService _dataService;
    private INavigationService _navigationService;

    private Statistics m_statistics;

    [ObservableProperty]
    private string loadedDictionaryName;
    [ObservableProperty]
    private string masteredTagsString = "";
    [ObservableProperty]
    private string masteredWordsString = "";
    [ObservableProperty]
    private ObservableCollection<TagStatistic> tagStatistics = [];
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewTagCommand))]
    private TagStatistic? selectedTag;

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
