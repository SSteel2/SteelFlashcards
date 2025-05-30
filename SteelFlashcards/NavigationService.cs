using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearn2
{
    public interface INavigationService
    {
        //string CurrentPage { get; }
        void NavigateTo(string page);
        void NavigateTo(string page, object parameter);
        void GoBack();
        void SetMainFrame(Frame mainFrame);
    }

    public class NavigationService : INavigationService
    {
        private readonly IDictionary<string, Type> _pages = new ConcurrentDictionary<string, Type>();

        private Frame MainFrame;

        //public string CurrentPage => throw new NotImplementedException();

        public NavigationService(Frame frame)
        {
            MainFrame = frame;
        }

        public NavigationService()
        {
        }

        public void SetMainFrame(Frame mainFrame)
        {
            this.MainFrame = mainFrame;
        }

        public void Configure(string page, Type type)
        {
            if (_pages.Values.Any(v => v == type))
            {
                throw new ArgumentException($"{type.Name} is already registered");
            }
            _pages[page] = type;
        }

        public void GoBack()
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        public void NavigateTo(string page)
        {
            NavigateTo(page, null);
        }

        public void NavigateTo(string page, object? parameter)
        {
            if (!_pages.ContainsKey(page))
            {
                throw new ArgumentException($"Page '{page}' not found");
            }
            MainFrame.Navigate(_pages[page], parameter);
        }
    }
}
