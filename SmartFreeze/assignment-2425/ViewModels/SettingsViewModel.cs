using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace assignment_2425.ViewModels
{
    // ViewModel for application settings and theme management
    public partial class SettingsViewModel : ObservableObject
    {
        public record ThemeOption(string Name, string Value);

        private ThemeOption? _selectedTheme;

        // Currently selected theme option
        public ThemeOption? SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged();
                    if (_selectedTheme != null)
                        App.SetTheme(_selectedTheme.Value);
                }
            }
        }

        // available theme options 
        public List<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption("System Default", "System"),
            new ThemeOption("Light Mode", "Light"),
            new ThemeOption("Dark Mode", "Dark")
        };

        // Load saved theme preference or default to system theme
        public SettingsViewModel()
        {
            var savedTheme = Preferences.Get("AppTheme", "System");
            SelectedTheme = ThemeOptions.FirstOrDefault(t => t.Value == savedTheme)
                ?? ThemeOptions[0];
        }
    }
}

