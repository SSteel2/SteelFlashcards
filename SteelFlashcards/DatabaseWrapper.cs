using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public class DictionaryFile
    {
        public string FileName { get; set; }

        public DictionaryEntry Content { get; set; }

        public bool IsLoaded { get; set; } = false;

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

        public int GetTagCount()
        {
            var tagSet = new HashSet<string>();
            foreach (var wordEntry in Content.WordEntries)
                foreach (string tag in wordEntry.Tags)
                    tagSet.Add(tag);
            return tagSet.Count;
        }
    }
}
