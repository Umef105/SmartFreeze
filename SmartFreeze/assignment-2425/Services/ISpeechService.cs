
namespace assignment_2425.Services
{
    // text-to-speech functionality
    public interface ISpeechService
    {
        Task SpeakAsync(string text);
        Task StopSpeakingAsync();
        Task<bool> RequestPermissionAsync();
        Task SpeakPageContentAsync(string title, string content);
    }
}
