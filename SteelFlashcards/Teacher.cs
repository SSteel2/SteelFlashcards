using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

namespace SteelFlashcards;

// Selects the next word to be learned based on the user's performance
internal class Teacher
{
    // Current set of words that are in learning session
    private readonly List<WordEntry> m_words;
    private readonly Statistics m_statistics;
    private Dictionary<WordMasteryLevel, List<WordEntry>> m_wordPools;
    private Queue<WordEntry> m_bannedWords = new();
    private int m_maxBannedWords;
    private WordMasteryLevel m_lastPool = WordMasteryLevel.Zero;

    private readonly Random m_random = new();
    // TODO: Make these values configurable
    private readonly Dictionary<WordMasteryLevel, int> c_poolWeights = new() {
        { WordMasteryLevel.Mastered,   1 },
        { WordMasteryLevel.Stale,      5 },
        { WordMasteryLevel.True_2,     5 },
        { WordMasteryLevel.True_1,    40 },
        { WordMasteryLevel.Zero,      20 },
        { WordMasteryLevel.False_1,   60 },
        { WordMasteryLevel.False_2,  120 }
    };

#if DEBUG
    private int m_wordGuessCount = 1;
#endif

    public Teacher(List<WordEntry> words, Statistics statistics)
    {
        m_words = words;
        m_statistics = statistics;
        CalculateMaxBannedWords();
        InitializeWordPools();
    }

    [MemberNotNull(nameof(m_wordPools))]
    private void InitializeWordPools()
    {
        m_wordPools = [];
        foreach (var poolName in Enum.GetValues<WordMasteryLevel>())
            m_wordPools[poolName] = [];
        foreach (var word in m_words)
            m_wordPools[m_statistics.CalculateWordMasteryLevel(word)].Add(word);
    }

    private void CalculateMaxBannedWords()
    {
        if (m_words.Count <= 1)
            m_maxBannedWords = 0;
        else if (m_words.Count <= 7)
            m_maxBannedWords = 1;
        else
            m_maxBannedWords = (int)Math.Log2(m_words.Count) - 1;
    }

    // Returns current pool weights based on actual words present.
    private Dictionary<WordMasteryLevel, int> GetPoolWeights()
    {
        // TODO: Apply scaling based on number of words
        Dictionary<WordMasteryLevel, int> currentPoolWeights = new(c_poolWeights);
        foreach (WordMasteryLevel pool in Enum.GetValues<WordMasteryLevel>())
        {
            if (m_wordPools[pool].Count == 0)
                currentPoolWeights[pool] = 0;
        }
        return currentPoolWeights;
    }

    private WordMasteryLevel GetRandomPool()
    {
        var poolWeights = GetPoolWeights();
        int totalWeight = poolWeights.Values.Sum();
        int randomValue = m_random.Next(totalWeight);
        int cumulativeWeight = 0;
        foreach (var pool in poolWeights)
        {
            cumulativeWeight += pool.Value;
            if (randomValue < cumulativeWeight)
                return pool.Key;
        }

        throw new InvalidOperationException("Dev: Failed to select a random pool.");
    }

    private WordEntry GetRandomWordFromPool(WordMasteryLevel pool)
    {
        var poolWords = m_wordPools[pool];
        if (poolWords.Count == 0)
            throw new InvalidOperationException($"Dev: No words in pool {pool}.");
        int randomIndex = m_random.Next(poolWords.Count);
        return poolWords[randomIndex];
    }

    public WordEntry GetNextWord()
    {
        m_lastPool = GetRandomPool();
        Debug.WriteLine($"[Teacher] ----------------------------- {m_wordGuessCount++,3} -----------------------------");
        Debug.WriteLine($"[Teacher] Current pool counts: {string.Join(", ", m_wordPools.Select(kvp => $"{kvp.Key}= {kvp.Value.Count}"))}.");
        Debug.WriteLine($"[Teacher] Selected pool: {m_lastPool} (words in pool: {m_wordPools[m_lastPool].Count}).");
        return GetRandomWordFromPool(m_lastPool);
    }

    public void AddAnswer(Answer answer)
    {
        // TODO: Reminder for myself, that duplicate words in dictionary will break stuff downstream
        WordEntry word = m_wordPools[m_lastPool].Single(entry => entry.Word == answer.Word);
        m_wordPools[m_lastPool].Remove(word);
        m_bannedWords.Enqueue(word);
        if (m_bannedWords.Count > m_maxBannedWords)
        {
            WordEntry permittedWord = m_bannedWords.Dequeue();
            WordMasteryLevel newLevel = m_statistics.CalculateWordMasteryLevel(permittedWord);
            m_wordPools[newLevel].Add(permittedWord);
        }
    }
}
