using assignment_2425.Services;
using assignment_2425.ViewModels;
using static Javax.Crypto.Spec.PSource;

namespace assignment_2425
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Load saved theme preference or default to system theme
            var savedTheme = Preferences.Get("AppTheme", "System");
            SetTheme(savedTheme);
            MainPage = new AppShell();
        }

        // Applies the specified theme and saves the preference
        public static void SetTheme(string theme)
        {
            var app = Current ?? throw new InvalidOperationException("Application.Current is null");

            switch (theme)
            {
                case "Dark":
                    Current.UserAppTheme = AppTheme.Dark;
                    break;
                case "Light":
                    Current.UserAppTheme = AppTheme.Light;
                    break;
                default:
                    Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
             Preferences.Set("AppTheme", theme);   
        }

        // Initializes database when app starts
        protected override async void OnStart()
        {
            var database = App.Current?.Handler?.MauiContext?.Services?.GetService<IDatabaseService>();
            if (database != null)
            {
                await database.InitializeAsync();
            }
        }

        protected override void OnSleep()
        {
            var fridgeVM = App.Current?.Handler?.MauiContext?.Services?.GetService<FridgeViewModel>();
            var groceryVM = App.Current?.Handler?.MauiContext?.Services?.GetService<GroceryViewModel>();

        }
    }
}
