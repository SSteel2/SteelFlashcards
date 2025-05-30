using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanguageLearn2
{
    public interface IDataService
    {
        IList<WordEntry> GetWords();
        WordEntry GetNextWord();
        void AddWordEntry(WordEntry wordEntry);
        void RemoveWordEntry(WordEntry wordEntry);
        IList<Answer> GetAnswers();
        Answer AddAnswer(string guess);
        void ClearAnswers();
        void Save();
    }

    public class DataService : IDataService
    {
        List<WordEntry> _words;
        int currentWordNumber;
        int wordCount;
        DictionaryEntry m_dictionary;

        IList<Answer> _answers;

        public DataService()
        {
            ReadDictJson();
            wordCount = _words.Count;
            currentWordNumber = -1;
            _answers = new List<Answer>();
        }

        public void Save()
        {
            m_dictionary.WordEntries = _words;
            string jsonString = JsonSerializer.Serialize(m_dictionary);
            string fileName = "dict.json";
            string fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", fileName);
            File.WriteAllText(fullPath, jsonString);
        }

        private void ReadDictJson()
        {
            string fileName = "dict.json";
            string fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Assets", fileName);
            string jsonString = File.ReadAllText(fullPath);
            m_dictionary = JsonSerializer.Deserialize<DictionaryEntry>(jsonString);
            _words = m_dictionary.WordEntries;
        }

        public WordEntry GetNextWord()
        {
            currentWordNumber = (currentWordNumber + 1) % wordCount;
            return _words[currentWordNumber];
        }

        public IList<Answer> GetAnswers()
        {
            return _answers;
        }

        public Answer AddAnswer(string guess)
        {
            int meaningIndex = _words[currentWordNumber].Meanings.IndexOf(guess);
            string correctAnswer;
            if (meaningIndex == -1)
                correctAnswer = _words[currentWordNumber].Meanings[0];
            else
                correctAnswer = _words[currentWordNumber].Meanings[meaningIndex];
            var answer = new Answer(_words[currentWordNumber].Word, correctAnswer, guess);
            _answers.Add(answer);
            return answer;
        }

        public void ClearAnswers()
        {
            _answers = new List<Answer>();
        }

        public IList<WordEntry> GetWords()
        {
            return _words;
        }

        public void AddWordEntry(WordEntry wordEntry)
        {
            _words.Add(wordEntry);
        }

        public void RemoveWordEntry(WordEntry wordEntry)
        {
            _words.Remove(wordEntry);
        }
    }
}
