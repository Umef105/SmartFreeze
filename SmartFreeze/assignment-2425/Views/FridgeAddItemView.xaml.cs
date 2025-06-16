using System.Diagnostics;
using assignment_2425.ViewModels;
using assignment_2425.Services;

namespace assignment_2425.Views;
public partial class FridgeAddItemView : ContentPage
{
    public FridgeAddItemView(FridgeViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        Debug.WriteLine($"FridgeAddItemView initialized with {viewModel.GetHashCode()}");
        Debug.WriteLine($"SearchResults count: {viewModel.SearchResults.Count}");

    }

    // Handles back navigation to main fridge page
    private async void OnBackClicked(object sender, EventArgs e)
    {
        HapticFeedbackService.PerformHapticFeedback();
        await Shell.Current.GoToAsync("//FridgePage");
    }

    // Provides haptic feedback during search text entry
    // Triggers only when typing new characters
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.NewTextValue) &&
        (e.OldTextValue == null || e.NewTextValue.Length > e.OldTextValue.Length))
        {
            HapticFeedbackService.PerformHapticFeedback();
        }
    }

    // Cleans up search state when leaving the page
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is FridgeViewModel viewModel)
        {
            viewModel.SearchResults.Clear();
            viewModel.SearchQuery = string.Empty;
        }
    }

}