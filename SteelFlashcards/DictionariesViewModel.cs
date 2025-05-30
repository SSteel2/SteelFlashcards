using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

        [ObservableProperty]
        private string dictionaryName;

        public DictionariesViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }

        // TODO: Missing CanExecute
        [RelayCommand]
        public void NewDictionary()
        {
            // TODO: exception handling is seriously missing in here
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationUserFolder = Path.Combine(documentsFolderPath, "Steel Flashcards");
            if (!Directory.Exists(applicationUserFolder))
                Directory.CreateDirectory(applicationUserFolder);
            string applicationUserDictionariesFolder = Path.Combine(applicationUserFolder, "Dictionaries");
            if (!Directory.Exists(@applicationUserDictionariesFolder))
                Directory.CreateDirectory(@applicationUserDictionariesFolder);
            var dictionary = new DictionaryEntry(DictionaryName, []);
            string serializedDictionary = JsonSerializer.Serialize(dictionary);
            string dictionaryFullPath = Path.Combine(applicationUserDictionariesFolder, SanitizeFileName(DictionaryName) + ".json");
            File.WriteAllText(dictionaryFullPath, serializedDictionary);
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
