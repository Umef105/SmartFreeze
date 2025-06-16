using assignment_2425.ViewModels;
using assignment_2425.Services;

namespace assignment_2425.Views;

public partial class GroceryAddItemView : ContentPage
{
    public GroceryAddItemView(GroceryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Handles back navigation to main grocery page
    private async void OnBackClicked(object sender, EventArgs e)
    {
        HapticFeedbackService.PerformHapticFeedback();
        await Shell.Current.GoToAsync("//GroceryPage");
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

        if (BindingContext is GroceryViewModel viewModel)
        {
            viewModel.SearchResults.Clear();
            viewModel.SearchQuery = string.Empty;
        }
    }
}