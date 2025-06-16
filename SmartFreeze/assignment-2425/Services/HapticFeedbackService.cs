using Microsoft.Maui.ApplicationModel;

namespace assignment_2425.Services
{
    // haptic feedback functionality
    public static class HapticFeedbackService
    {
        public static void PerformHapticFeedback(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            try
            {
                // Only execute if device supports haptics
                if (HapticFeedback.Default.IsSupported)
                {
                    HapticFeedback.Default.Perform(type);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Haptic feedback error: {ex.Message}");
            }
        
        }
    }
}
