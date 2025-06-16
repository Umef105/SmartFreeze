using assignment_2425.Services;

namespace assignment_2425.Views;

public partial class MainPageView : ContentPage
{
    public MainPageView()
    {
        InitializeComponent();
    }

    // Handles navigation to fridge page
    private async void GetStartedClicked(object sender, EventArgs e)
    {
        HapticFeedbackService.PerformHapticFeedback();
        await Shell.Current.GoToAsync("//FridgePage");


    }
}
