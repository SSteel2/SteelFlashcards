using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanguageLearn2
{
    public partial class DictionariesViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        private ObservableCollection<DictionaryFile> m_dictionaryFiles;
        public ObservableCollection<DictionaryFile> DictionaryFiles { get { return m_dictionaryFiles; } }

        [ObservableProperty]
        private string newDictionaryName;

        [ObservableProperty]
        private string renameDictionaryName;

        [ObservableProperty]
        private DictionaryFile? selectedDictionary;

        [ObservableProperty]
        private DictionaryFile? loadedDictionary;

        public DictionariesViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            m_dictionaryFiles = [];
            ReadDictionaries();
            // TODO: edge case if no dictionaries exist, copy template
            // For simplicity sake, the first dictionary is the selected dictionary for now
            selectedDictionary = m_dictionaryFiles[0];
            LoadedDictionary = m_dictionaryFiles[0];
            m_dictionaryFiles[0].IsLoaded = true;
            //DictionaryName = selectedDictionary.Content.Name;
        }

        // TODO: Missing CanExecute
        [RelayCommand]
        public void NewDictionary()
        {
            if (string.IsNullOrWhiteSpace(NewDictionaryName))
                return; // TODO: maybe some better indication of bad name
            string applicationUserDictionariesFolder = GetDictionariesDirectory();
            var dictionary = new DictionaryEntry(NewDictionaryName, []);
            string serializedDictionary = JsonSerializer.Serialize(dictionary);
            string dictionaryFullPath = Path.Combine(applicationUserDictionariesFolder, SanitizeFileName(NewDictionaryName) + ".json");
            // TODO: edge case. Check if file exists
            File.WriteAllText(dictionaryFullPath, serializedDictionary);
            var dictionaryFile = ReadDictionary(dictionaryFullPath);
            if (dictionaryFile != null)
                m_dictionaryFiles.Add(dictionaryFile);
        }

        [RelayCommand]
        public void RenameDictionary()
        {
            if (string.IsNullOrWhiteSpace(RenameDictionaryName) || string.Equals(SelectedDictionary.Content.Name, RenameDictionaryName))
                return; // TODO: maybe some better indication of bad name
            SelectedDictionary.Content.Name = RenameDictionaryName;
            SelectedDictionary.DictionaryName = RenameDictionaryName;
            string serializedDictionary = JsonSerializer.Serialize(SelectedDictionary.Content);
            File.WriteAllText(SelectedDictionary.FileName, serializedDictionary);
        }

        [RelayCommand]
        public void DeleteDictionary()
        {
            if (SelectedDictionary == null)
                return;
            bool isDeletedLoaded = SelectedDictionary == LoadedDictionary;
            File.Delete(SelectedDictionary.FileName);
            m_dictionaryFiles.Remove(SelectedDictionary);
            if (isDeletedLoaded)
            {
                if (m_dictionaryFiles.Count > 0)
                {
                    LoadedDictionary = m_dictionaryFiles[0];
                    LoadedDictionary.IsLoaded = true;
                }
                else
                {
                    LoadedDictionary = null;
                }
            }
        }

        [RelayCommand]
        public void LoadDictionary()
        {
            if (SelectedDictionary == LoadedDictionary)
                return;

            if (LoadedDictionary != null)
                LoadedDictionary.IsLoaded = false;
            LoadedDictionary = SelectedDictionary;
            LoadedDictionary.IsLoaded = true;
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

        private DictionaryFile? ReadDictionary(string fileName)
        {
            string content = File.ReadAllText(fileName);
            DictionaryEntry? dictionaryEntry = JsonSerializer.Deserialize<DictionaryEntry>(content);
            if (dictionaryEntry == null)
                return null;
            // TODO: what happens on malformed files - exception handling needed
            var dictionaryFile = new DictionaryFile
            {
                Content = dictionaryEntry,
                DictionaryName = dictionaryEntry.Name,
                FileName = fileName
            };
            return dictionaryFile;
        }

        private string GetDictionariesDirectory()
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

        private string SanitizeFileName(string fileName)
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
