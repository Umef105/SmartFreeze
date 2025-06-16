using assignment_2425.ViewModels;

namespace assignment_2425.Views;

public partial class FridgeView : ContentPage
{
	public FridgeView(FridgeViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
