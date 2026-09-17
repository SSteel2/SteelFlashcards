using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SteelFlashcards;

public interface INavigationService
{
    void NavigateTo(string page);
    void NavigateTo(string page, object parameter);
    void GoBack();
    void SetMainFrame(Frame mainFrame);
}

public class NavigationService : INavigationService
{
    private readonly IDictionary<string, Type> m_pages = new ConcurrentDictionary<string, Type>();

    private Frame? MainFrame;

    public NavigationService()
    {
    }

    public void SetMainFrame(Frame mainFrame)
    {
        MainFrame = mainFrame;
    }

    public void Configure(string page, Type type)
    {
        if (m_pages.Values.Any(v => v == type))
        {
            throw new ArgumentException($"{type.Name} is already registered");
        }
        m_pages[page] = type;
    }

    public void GoBack()
    {
        if (MainFrame?.CanGoBack == true)
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
        if (!m_pages.ContainsKey(page))
        {
            throw new ArgumentException($"Page '{page}' not found");
        }
        MainFrame?.Navigate(m_pages[page], parameter);
    }
}
