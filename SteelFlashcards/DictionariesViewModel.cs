﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace LanguageLearn2
{
    public partial class DictionariesViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        private ObservableCollection<DictionaryFile> m_dictionaryFiles;
        public ObservableCollection<DictionaryFile> DictionaryFiles { get { return m_dictionaryFiles; } }

        [ObservableProperty]
        private string? newDictionaryName;

        [ObservableProperty]
        private string? renameDictionaryName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(LoadDictionaryCommand))]
        private DictionaryFile? selectedDictionary;

        [ObservableProperty]
        private DictionaryFile? loadedDictionary;

        public DictionariesViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            
            m_dictionaryFiles = [];
            foreach (var dictionaryFile in _dataService.GetDictionaries())
                m_dictionaryFiles.Add(dictionaryFile);
            LoadedDictionary = _dataService.GetLoadedDictionary();
        }

        // TODO: Missing CanExecute
        [RelayCommand]
        public void NewDictionary()
        {
            if (string.IsNullOrWhiteSpace(NewDictionaryName))
                return; // TODO: maybe some better indication of bad name
            
            var dictionaryFile = _dataService.NewDictionary(NewDictionaryName);
            if (dictionaryFile != null)
                m_dictionaryFiles.Add(dictionaryFile);
        }

        [RelayCommand(CanExecute = nameof(IsDictionarySelected))]
        public void RenameDictionary()
        {
            if (string.IsNullOrWhiteSpace(RenameDictionaryName) || string.Equals(SelectedDictionary!.Content.Name, RenameDictionaryName))
                return; // TODO: maybe some better indication of bad name

            _dataService.RenameDictionary(SelectedDictionary, RenameDictionaryName);
        }

        [RelayCommand(CanExecute = nameof(IsDictionarySelected))]
        public void DeleteDictionary()
        {
            if (SelectedDictionary == null)
                return;
            _dataService.DeleteDictionary(SelectedDictionary);
            m_dictionaryFiles.Remove(SelectedDictionary);
            LoadedDictionary = _dataService.GetLoadedDictionary();
        }

        [RelayCommand(CanExecute = nameof(IsDictionarySelected))]
        public void LoadDictionary()
        {
            if (SelectedDictionary == LoadedDictionary)
                return;

            _dataService.LoadDictionary(SelectedDictionary!);
            LoadedDictionary = _dataService.GetLoadedDictionary();
        }

        private bool IsDictionarySelected()
        {
            return SelectedDictionary != null;
        }
    }
}
