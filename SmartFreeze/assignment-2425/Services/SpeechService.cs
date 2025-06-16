using System.Diagnostics;
using Microsoft.Maui.Media;

namespace assignment_2425.Services
{
    // text-to-speech functionality
    public class SpeechService : ISpeechService
    {
        private CancellationTokenSource? _currentSpeechCts;

        public async Task SpeakAsync(string text)
        {
            try
            {
                // Cancel any ongoing speech
                _currentSpeechCts?.Cancel();
                _currentSpeechCts = new CancellationTokenSource();


                var locales = await TextToSpeech.Default.GetLocalesAsync();
                var locale = locales.FirstOrDefault(l => l.Language.StartsWith("en"));

                if (locale == null)
                {
                    Debug.WriteLine("English locale not found. Using default locale.");
                    locale = locales.FirstOrDefault();
                }

                var options = new SpeechOptions()
                {
                    Pitch = 1.0f,
                    Volume = 1.0f,
                    Locale = locale
                };

                await TextToSpeech.Default.SpeakAsync(text, options, _currentSpeechCts.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Speech failed: {ex.Message}");
            }
        }

        // Stops any currently playing speech
        public Task StopSpeakingAsync()
        {
            try
            {
                _currentSpeechCts?.Cancel();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to stop speech: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        public async Task SpeakPageContentAsync(string title, string content)
        {
            try
            {
                var fullText = $"{title}. {content}";
                await SpeakAsync(fullText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Page reading failed: {ex}");
            }
        }

        // Requests and validates speech permission
        public async Task<bool> RequestPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Speech>();

            if (status == PermissionStatus.Denied)
            {
                AppInfo.Current.ShowSettingsUI();
                return false;
            }

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Speech>();

                if (status == PermissionStatus.Denied)
                {
                    await Shell.Current.DisplayAlert("Permission Required",
                        "Speech permission is needed to read content aloud", "OK");
                    return false;
                }
            }
            return status == PermissionStatus.Granted;
        }
    }
}
