using assignment_2425.ViewModels;

namespace assignment_2425.Views;

public partial class SettingsView : ContentPage
{
	public SettingsView()
	{
		InitializeComponent();
		BindingContext = new SettingsViewModel();
	}
}