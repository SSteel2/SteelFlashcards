using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SteelFlashcards;

public class WordEntry(string word, List<string> meanings, List<string> tags)
{
    [JsonPropertyName("id")] public int Id { get; set; } = -1;
    [JsonPropertyName("word")] public string Word { get; set; } = word;
    [JsonPropertyName("meanings")] public List<string> Meanings { get; set; } = meanings;
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = tags;

    public string GetMeaningsString()
    {
        return string.Join("; ", Meanings);
    }

    public string GetTagsString()
    {
        return string.Join("; ", Tags);
    }

    // TODO: This could be an extension method
    public static WordEntry? FindWordEntry(IList<WordEntry> words, int id)
    {
        foreach (WordEntry word in words)
            if (word.Id == id) 
                return word;
        return null;
    }
}

public class DictionaryEntry(string name, List<WordEntry> wordEntries)
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = name;
    [JsonPropertyName("words")]
    public List<WordEntry> WordEntries { get; set; } = wordEntries;

    public WordEntry NewWordEntry(string word, List<string> meanings, List<string> tags)
    {
        var wordEntry = new WordEntry(word, meanings, tags)
        {
            Id = FindMaxId() + 1
        };
        WordEntries.Add(wordEntry);
        return wordEntry;
    }

    public bool ContainsDuplicates()
    {
        HashSet<string> duplicateCheck = [];
        foreach (var entry in WordEntries)
        {
            if (!duplicateCheck.Add(entry.Word))
                return true;
        }
        return false;
    }

    public bool SanitizeIds()
    {
        int nextId = FindMaxId() + 1;
        bool changed = false;
        HashSet<int> allIds = [];
        foreach (var entry in WordEntries)
        {
            if (entry.Id == -1 || !allIds.Add(entry.Id))
            {
                entry.Id = nextId++;
                changed = true;
            }
        }
        return changed;
    }

    private int FindMaxId()
    {
        int maxId = -1;
        foreach (var entry in WordEntries)
        {
            if (entry.Id > maxId)
                maxId = entry.Id;
        }
        return maxId;
    }
}

public partial class DictionaryFile : ObservableObject
{
    public required string FileName { get; set; }
    public DictionaryEntry Content { get; set; }

    [ObservableProperty] public partial bool IsLoaded { get; set; }
    [ObservableProperty] public partial string DictionaryName { get; set; }
    [ObservableProperty] public partial bool IsPristine { get; set; }

    public int WordCount { get { return Content.WordEntries.Count; } }
    public int TagCount { get { return GetTagCount(); } }

    public DictionaryFile(DictionaryEntry content)
    {
        Content = content;
        DictionaryName = content.Name;
        IsPristine = !Content.SanitizeIds() && !Content.ContainsDuplicates();
    }

    // Calculated on the fly, not stored in memory (TODO: yet)
    public List<DictionaryTag> GetTags()
    {
        Dictionary<string, int> tags = [];
        foreach (var wordEntry in Content.WordEntries)
        {
            foreach (string tag in wordEntry.Tags)
            {
                if (tags.TryGetValue(tag, out int value))
                    tags[tag] = ++value;
                else
                    tags[tag] = 1;
            }
        }
        List<DictionaryTag> tagList = [];
        foreach (KeyValuePair<string, int> tag in tags)
            tagList.Add(new DictionaryTag(tag.Key, tag.Value));
        return tagList;
    }

    public int GetTagCount()
    {
        var tagSet = new HashSet<string>();
        foreach (var wordEntry in Content.WordEntries)
            foreach (string tag in wordEntry.Tags)
                tagSet.Add(tag);
        return tagSet.Count;
    }
}

public class DictionaryTag(string tagName, int wordCount)
{
    public string TagName { get; set; } = tagName;
    public int WordCount { get; set; } = wordCount;
}
