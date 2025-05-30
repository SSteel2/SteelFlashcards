using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearn2
{
    public partial class DictionariesViewModel : ObservableObject
    {
        private IDataService _dataService;
        private INavigationService _navigationService;

        public DictionariesViewModel(IDataService dataService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
        }
    }
}
