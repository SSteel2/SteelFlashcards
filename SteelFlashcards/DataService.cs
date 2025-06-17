using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace LanguageLearn2
{
    public interface IDataService
    {
        IList<WordEntry> GetWords();
        IList<DictionaryTag> GetActiveTags();
        void SetActiveTags(IList<DictionaryTag> tags);
        void AddWordEntry(WordEntry wordEntry);
        void RemoveWordEntry(WordEntry wordEntry);
        IList<Answer> GetAnswers();
        void AddAnswer(LearnPageAnswer answer);
        Task FlushAnswers();
        void Save();
        DictionaryFile? NewDictionary(string newName);
        void RenameDictionary(DictionaryFile dictionary, string newName);
        void DeleteDictionary(DictionaryFile dictionary);
        Task ExportDictionaryAsync(DictionaryFile dictionary, StorageFile storageFile);
        Task<DictionaryFile?> ImportDictionaryAsync(StorageFile storageFile);
        void LoadDictionary(DictionaryFile dictionary);
        IList<DictionaryFile> GetDictionaries();
        DictionaryFile? GetLoadedDictionary();

        // temp (until I decide on a proper way where to store data)
        Statistics GetStatistics();
        TagStatistic? GetTagStatistic(string tagName);
    }

    public class DataService : IDataService
    {
        List<WordEntry> _words = [];

        List<Answer> _answers;
        List<Answer> _answersBuffer;
        bool m_isAnswersLoaded;
        List<DictionaryTag> m_activeTags = [];

        List<DictionaryFile> m_dictionaryFiles = [];
        DictionaryFile? m_loadedDictionary = null;

        Statistics m_statistics = new();

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

            _answers = [];
            _answersBuffer = [];
            m_isAnswersLoaded = false;

            InitializeDummyStatistics();
        }

        public void Save()
        {
            if (m_loadedDictionary == null)
                throw new ApplicationException("Loaded dictionary is null in Save method");
            Save(m_loadedDictionary);
        }

        public IList<Answer> GetAnswers()
        {
            // Redo
            return _answers;
        }

        public void AddAnswer(LearnPageAnswer answer)
        {
            _answersBuffer.Add(answer);
        }

        public async Task FlushAnswers()
        {
            // Nothing to save
            if (_answersBuffer.Count == 0)
                return;
            // Should never happen
            if (m_loadedDictionary == null)
                return;

            // TODO: seriously needs to be more robust
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(GetAnswersFolderPath());
            // Answers file matches dictionary file name in answers directory
            string fileName = Path.GetFileNameWithoutExtension(m_loadedDictionary.FileName);
            fileName += "_answers.json";
            StorageFile? answersFile = await storageFolder.TryGetItemAsync(fileName) as StorageFile;
            // File didn't exist
            if (answersFile == null)
            {
                answersFile = await storageFolder.CreateFileAsync(fileName);
                m_isAnswersLoaded = true;
            }
            if (!m_isAnswersLoaded)
            {
                string answersContent = await FileIO.ReadTextAsync(answersFile);
                _answers = JsonSerializer.Deserialize<List<Answer>>(answersContent) ?? throw new ArgumentException("Answers content is malformed");
                m_isAnswersLoaded = true;
            }
            _answers.AddRange(_answersBuffer);
            string updatedAnswersContent = JsonSerializer.Serialize(_answers);
            await FileIO.WriteTextAsync(answersFile, updatedAnswersContent);
            _answersBuffer.Clear();
        }

        public IList<WordEntry> GetWords()
        {
            return _words;
        }

        public IList<DictionaryTag> GetActiveTags()
        {
            return m_activeTags;
        }

        public void SetActiveTags(IList<DictionaryTag> tags)
        {
            m_activeTags.Clear();
            m_activeTags.AddRange(tags);
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
            string applicationUserDictionariesFolder = GetDictionariesFolderPath();
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

        public async Task ExportDictionaryAsync(DictionaryFile dictionary, StorageFile storageFile)
        {
            CachedFileManager.DeferUpdates(storageFile);
            string jsonString = JsonSerializer.Serialize(dictionary.Content);
            await FileIO.WriteTextAsync(storageFile, jsonString);
            await CachedFileManager.CompleteUpdatesAsync(storageFile);
        }

        public async Task<DictionaryFile?> ImportDictionaryAsync(StorageFile storageFile)
        {
            string jsonString = await FileIO.ReadTextAsync(storageFile);
            DictionaryEntry? dictionaryEntry = JsonSerializer.Deserialize<DictionaryEntry>(jsonString);
            if (dictionaryEntry == null)
                return null;

            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(GetDictionariesFolderPath());
            StorageFile newDictionary = await storageFile.CopyAsync(storageFolder, storageFile.Name, NameCollisionOption.GenerateUniqueName);

            var dictionaryFile = ReadDictionary(newDictionary.Path);
            if (dictionaryFile != null)
                m_dictionaryFiles.Add(dictionaryFile);
            return dictionaryFile;
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

        public Statistics GetStatistics()
        {
            return m_statistics;
        }

        public TagStatistic? GetTagStatistic(string tagName)
        {
            if (!m_statistics.tags.ContainsKey(tagName))
                return null; // TODO: should not happen, needs proper error handling?
            return m_statistics.tags[tagName];
        }

        private void ReadDictionaries()
        {
            string applicationUserDictionariesFolder = GetDictionariesFolderPath();
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
        private static void Save(DictionaryFile dictionaryFile)
        {
            string jsonString = JsonSerializer.Serialize(dictionaryFile.Content);
            File.WriteAllText(dictionaryFile.FileName, jsonString);
        }

        private void CopyTemplateDictionary()
        {
            string fileName = "template_dict.json";
            string fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "Assets", fileName);
            string destinationPath = Path.Combine(GetDictionariesFolderPath(), "default_dict.json");
            File.Copy(fullPath, destinationPath);
        }

        private static string GetDictionariesFolderPath()
        {
            // TODO: remake string into enum (maybe)
            return GetApplicationUserFolderPath("Dictionaries");
        }

        private static string GetAnswersFolderPath()
        {
            return GetApplicationUserFolderPath("Answers");
        }

        private static string GetApplicationUserFolderPath(string subDirectoryName)
        {
            string applicationUserFolder = GetApplicationUserFolderPath();
            string applicationUserDictionariesFolder = Path.Combine(applicationUserFolder, subDirectoryName);
            if (!Directory.Exists(@applicationUserDictionariesFolder))
                Directory.CreateDirectory(@applicationUserDictionariesFolder);
            return applicationUserDictionariesFolder;
        }

        private static string GetApplicationUserFolderPath()
        {
            // TODO: exception handling is seriously missing in here
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationUserFolder = Path.Combine(documentsFolderPath, "Steel Flashcards");
            if (!Directory.Exists(applicationUserFolder))
                Directory.CreateDirectory(applicationUserFolder);
            return applicationUserFolder;
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

        private void InitializeDummyStatistics()
        {
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
