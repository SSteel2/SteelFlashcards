using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LanguageLearn2
{
    public partial class EditViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        private ObservableCollection<WordEntry> _wordEntries;
        public ObservableCollection<WordEntry> WordEntries { get { return _wordEntries; } }

        private WordEntry? m_currentEditWord;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptWordEntryCommand))]
        private string? newWordText;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptWordEntryCommand))]
        private string? newMeaningText;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptWordEntryCommand))]
        private string? newTagsText;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool isDirty = false;

        public EditViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _wordEntries = new ObservableCollection<WordEntry>(_dataService.GetWords());
            m_currentEditWord = null;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteAcceptWordEntry))]
        private void AcceptWordEntry()
        {
            AcceptWordEntryInternal();
        }

        public WordEntry AcceptWordEntryInternal()
        {
            WordEntry changedWordEntry;
            if (m_currentEditWord == null)
            {
                var wordEntry = new WordEntry(NewWordText, [.. NewMeaningText.Split("; ")], [.. NewTagsText.Split("; ")]);
                _dataService.AddWordEntry(wordEntry);
                _wordEntries.Add(wordEntry);
                changedWordEntry = wordEntry;
            }
            else
            {
                m_currentEditWord.Word = NewWordText;
                m_currentEditWord.Meanings = [.. NewMeaningText.Split("; ").Where(x => !string.IsNullOrWhiteSpace(x))];
                m_currentEditWord.Tags = [.. NewTagsText.Split("; ").Where(x => !string.IsNullOrWhiteSpace(x))];

                // This is stupid, I know, but I didn't find a better way of notifying the collection
                int index = _wordEntries.IndexOf(m_currentEditWord);
                _wordEntries.RemoveAt(index);
                _wordEntries.Insert(index, m_currentEditWord);
                changedWordEntry = m_currentEditWord;
            }

            ClearEditFields();
            IsDirty = true;
            return changedWordEntry;
        }

        public bool CanExecuteAcceptWordEntry()
        {
            return !string.IsNullOrWhiteSpace(NewWordText) && !string.IsNullOrWhiteSpace(NewMeaningText) && !string.IsNullOrWhiteSpace(NewTagsText);
        }

        public void EditWordEntry(int wordEntryId)
        {
            var word = WordEntry.FindWordEntry(_wordEntries, wordEntryId) ?? throw new ApplicationException($"Word entry (Id: {wordEntryId}) not found");
            m_currentEditWord = word;
            NewWordText = word.Word;
            NewMeaningText = word.GetMeaningsString();
            NewTagsText = word.GetTagsString();
        }

        public void DeleteWordEntry(int wordEntryId)
        {
            var word = WordEntry.FindWordEntry(_wordEntries, wordEntryId) ?? throw new ApplicationException($"Word entry (Id: {wordEntryId}) not found");
            if (word == m_currentEditWord)
            {
                ClearEditFields();
            }
            _wordEntries.Remove(word);
            IsDirty = true;
        }

        [RelayCommand]
        public void RejectWordEntry()
        {
            ClearEditFields();
        }

        private void ClearEditFields()
        {
            NewWordText = string.Empty;
            NewMeaningText = string.Empty;
            NewTagsText = string.Empty;
            m_currentEditWord = null;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSave))]
        public void Save()
        {
            _dataService.Save();
            IsDirty = false;
        }

        private bool CanExecuteSave()
        {
            return IsDirty;
        }
    }
}
