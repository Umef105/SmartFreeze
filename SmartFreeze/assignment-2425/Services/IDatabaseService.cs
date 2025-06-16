using assignment_2425.ViewModels;

namespace assignment_2425.Services
{
    // database operations for fridge and grocery items
    public interface IDatabaseService
    {
        // Fridge Item Operations
        Task<List<FridgeViewModel.FridgeItem>> GetFridgeItemsAsync();
        Task<int> SaveFridgeItemAsync(FridgeViewModel.FridgeItem item);
        Task<int> DeleteFridgeItemAsync(FridgeViewModel.FridgeItem item);

        // Grocery Item Operations
        Task<List<GroceryViewModel.GroceryItem>> GetGroceryItemsAsync();
        Task<int> SaveGroceryItemAsync(GroceryViewModel.GroceryItem item);
        Task<int> DeleteGroceryItemAsync(GroceryViewModel.GroceryItem item);

        Task InitializeAsync();
        Task ClearAllGroceryItems();
    }
}
