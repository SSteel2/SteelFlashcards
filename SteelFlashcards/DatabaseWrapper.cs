using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace LanguageLearn2
{
    public class WordEntry
    {
        [JsonPropertyName("word")]
        public string Word { get; set; }
        
        [JsonPropertyName("meanings")]
        public List<string> Meanings { get; set; }
        
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonIgnore]
        public int LocalId { get; }

        private static int s_currentId = 0;

        public WordEntry(string word, List<string> meanings, List<string> tags)
        {
            Word = word;
            Meanings = meanings;
            Tags = tags;
            LocalId = s_currentId++;
        }

        public string GetMeaningsString()
        {
            return string.Join("; ", Meanings);
        }

        public string GetTagsString()
        {
            return string.Join("; ", Tags);
        }

        public static WordEntry? FindWordEntry(IList<WordEntry> words, int id)
        {
            foreach (WordEntry word in words)
                if (word.LocalId == id) 
                    return word;
            return null;
        }
    }

    public class DictionaryEntry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("words")]
        public List<WordEntry> WordEntries { get; set; }

        public DictionaryEntry(string name, List<WordEntry> wordEntries)
        {
            Name = name;
            WordEntries = wordEntries;
        }
    }

    public partial class DictionaryFile : ObservableObject
    {
        public string FileName { get; set; }

        public DictionaryEntry Content { get; set; }

        [ObservableProperty]
        private bool isLoaded;

        [ObservableProperty]
        private string dictionaryName;

        public int WordCount { get { return Content.WordEntries.Count; } }

        private int m_tagCount = -1;
        public int TagCount
        {
            get
            {
                if (m_tagCount == -1)
                    m_tagCount = GetTagCount();
                return m_tagCount;
            }
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
}
