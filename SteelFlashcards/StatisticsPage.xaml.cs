using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StatisticsPage : Page
{
    public StatisticsViewModel ViewModel;
    
    public StatisticsPage()
    {
        var viewModel = App.ServiceProvider?.GetService<StatisticsViewModel>();
        if (viewModel == null)
            throw new ApplicationException("Dev: Missing StatisticsViewModel Service");
        ViewModel = viewModel;
        InitializeComponent();
    }
}
