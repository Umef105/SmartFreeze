using Microsoft.Extensions.Logging;
using assignment_2425.Services;
using assignment_2425.ViewModels;
using assignment_2425.Views;


namespace assignment_2425
{
    public static class MauiProgram
    {
        public static IServiceProvider? Services {  get; private set; }  
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<IHapticFeedback>(HapticFeedback.Default);
            builder.Services.AddSingleton<ISpeechService, SpeechService>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Register ViewModels
            builder.Services.AddSingleton<FoodDataService>();
            builder.Services.AddSingleton<FridgeViewModel>();
            builder.Services.AddSingleton<GroceryViewModel>();

            // Register Views
            builder.Services.AddTransient<FridgeAddItemView>();
            builder.Services.AddTransient<FridgeView>();
            builder.Services.AddTransient<GroceryView>();
            builder.Services.AddTransient<GroceryAddItemView>();

            var app = builder.Build();
            Services = app.Services;

            return app;

        }
    }
}
