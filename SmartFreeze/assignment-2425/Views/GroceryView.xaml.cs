using assignment_2425.ViewModels;

namespace assignment_2425.Views;

public partial class GroceryView : ContentPage
{
    public GroceryView(GroceryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GroceryViewModel vm)
        {
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is GroceryViewModel vm)
        {
            vm.Dispose();
        }
        base.OnDisappearing();
    }

}
