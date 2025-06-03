using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LanguageLearn2
{
    public interface IDataService
    {
        IList<WordEntry> GetWords();
        void AddWordEntry(WordEntry wordEntry);
        void RemoveWordEntry(WordEntry wordEntry);
        IList<Answer> GetAnswers();
        void AddAnswer(Answer answer);
        void ClearAnswers();
        void Save();
        DictionaryFile? NewDictionary(string newName);
        void RenameDictionary(DictionaryFile dictionary, string newName);
        void DeleteDictionary(DictionaryFile dictionary);
        void LoadDictionary(DictionaryFile dictionary);
        IList<DictionaryFile> GetDictionaries();
        DictionaryFile? GetLoadedDictionary();
    }

    public class DataService : IDataService
    {
        List<WordEntry> _words = [];

        IList<Answer> _answers;

        List<DictionaryFile> m_dictionaryFiles = [];
        DictionaryFile? m_loadedDictionary = null;

        public DataService()
        {
            ReadDictionaries();
            if (m_dictionaryFiles.Count == 0)
            {
                CopyTemplateDictionary();
                ReadDictionaries();
            }
            // TODO: For simplicity, the first dictionary is the selected dictionary for now
            if (m_dictionaryFiles.Count == 0)
            {
                throw new ApplicationException("Template dictionary could not be copied");
            }
            m_loadedDictionary = m_dictionaryFiles[0];
            m_loadedDictionary.IsLoaded = true;
            _words = m_loadedDictionary.Content.WordEntries;

            _answers = new List<Answer>();
        }

        public void Save()
        {
            if (m_loadedDictionary == null)
                throw new ApplicationException("Loaded dictionary is null in Save method");
            Save(m_loadedDictionary);
        }

        private static void Save(DictionaryFile dictionaryFile)
        {
            string jsonString = JsonSerializer.Serialize(dictionaryFile.Content);
            File.WriteAllText(dictionaryFile.FileName, jsonString);
        }

        private void CopyTemplateDictionary()
        {
            string fileName = "template_dict.json";
            string fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "Assets", fileName);
            string destinationPath = Path.Combine(GetDictionariesDirectory(), "default_dict.json");
            File.Copy(fullPath, destinationPath);
        }

        public IList<Answer> GetAnswers()
        {
            return _answers;
        }

        public void AddAnswer(Answer answer)
        {
            _answers.Add(answer);
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

        public DictionaryFile? NewDictionary(string newName)
        {
            string applicationUserDictionariesFolder = GetDictionariesDirectory();
            var dictionary = new DictionaryEntry(newName, []);
            string serializedDictionary = JsonSerializer.Serialize(dictionary);
            string dictionaryFullPath = Path.Combine(applicationUserDictionariesFolder, SanitizeFileName(newName) + ".json");
            // TODO: edge case. Check if file exists
            File.WriteAllText(dictionaryFullPath, serializedDictionary);
            var dictionaryFile = ReadDictionary(dictionaryFullPath);
            if (dictionaryFile != null)
                m_dictionaryFiles.Add(dictionaryFile);
            return dictionaryFile;
        }

        public void RenameDictionary(DictionaryFile dictionary, string newName)
        {
            dictionary.DictionaryName = newName;
            dictionary.Content.Name = newName;
            Save(dictionary);
        }

        public void DeleteDictionary(DictionaryFile dictionary)
        {
            bool isDeletedLoaded = dictionary == m_loadedDictionary;
            File.Delete(dictionary.FileName);
            m_dictionaryFiles.Remove(dictionary);
            if (isDeletedLoaded)
            {
                if (m_dictionaryFiles.Count > 0)
                {
                    m_loadedDictionary = m_dictionaryFiles[0];
                    m_loadedDictionary.IsLoaded = true;
                }
                else
                {
                    m_loadedDictionary = null;
                }
            }
        }

        public void LoadDictionary(DictionaryFile dictionary)
        {
            if (m_loadedDictionary != null)
                m_loadedDictionary.IsLoaded = false;
            m_loadedDictionary = dictionary;
            m_loadedDictionary.IsLoaded = true;
            _words = m_loadedDictionary.Content.WordEntries;
        }

        public DictionaryFile? GetLoadedDictionary()
        {
            return m_loadedDictionary;
        }

        public IList<DictionaryFile> GetDictionaries()
        {
            return m_dictionaryFiles;
        }

        private void ReadDictionaries()
        {
            string applicationUserDictionariesFolder = GetDictionariesDirectory();
            string[] allFiles = Directory.GetFiles(applicationUserDictionariesFolder);
            foreach (string file in allFiles)
            {
                var dictionaryFile = ReadDictionary(file);
                if (dictionaryFile != null)
                {
                    m_dictionaryFiles.Add(dictionaryFile);
                }
            }
        }

        private static DictionaryFile? ReadDictionary(string fileName)
        {
            string content = File.ReadAllText(fileName);
            DictionaryEntry? dictionaryEntry = JsonSerializer.Deserialize<DictionaryEntry>(content);
            if (dictionaryEntry == null)
                return null;
            // TODO: what happens on malformed files - exception handling needed
            return new DictionaryFile
            {
                Content = dictionaryEntry,
                DictionaryName = dictionaryEntry.Name,
                FileName = fileName
            };
        }

        private static string GetDictionariesDirectory()
        {
            // TODO: exception handling is seriously missing in here
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationUserFolder = Path.Combine(documentsFolderPath, "Steel Flashcards");
            if (!Directory.Exists(applicationUserFolder))
                Directory.CreateDirectory(applicationUserFolder);
            string applicationUserDictionariesFolder = Path.Combine(applicationUserFolder, "Dictionaries");
            if (!Directory.Exists(@applicationUserDictionariesFolder))
                Directory.CreateDirectory(@applicationUserDictionariesFolder);
            return applicationUserDictionariesFolder;
        }

        private static string SanitizeFileName(string fileName)
        {
            string sanitizedFileName = fileName;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sanitizedFileName = sanitizedFileName.Replace(c, '_');
            }
            return sanitizedFileName;
        }

    }
}
