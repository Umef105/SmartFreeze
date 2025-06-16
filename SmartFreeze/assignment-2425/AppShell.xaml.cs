using assignment_2425.Views;

namespace assignment_2425
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
           InitializeComponent();

            // Register all navigation routes
            Routing.RegisterRoute("FridgePage", typeof(FridgeView));
            Routing.RegisterRoute("GroceryPage", typeof(GroceryView));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsView));
            Routing.RegisterRoute("FridgeAddItemView", typeof(FridgeAddItemView));
            Routing.RegisterRoute("GroceryAddItemView", typeof(GroceryAddItemView));
        }
    }
}
  