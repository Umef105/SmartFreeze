using SQLite;
using assignment_2425.Models;
using assignment_2425.ViewModels;

namespace assignment_2425.Services
{
    // fridge and grocery items database 
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public DatabaseService()
        {
        }

        public async Task InitializeAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            // create tables
            await _database.CreateTableAsync<FridgeViewModel.FridgeItem>();
            await _database.CreateTableAsync<GroceryViewModel.GroceryItem>();
        }


        #region Fridge Items

        // get all fridge items from the database
        public async Task<List<FridgeViewModel.FridgeItem>> GetFridgeItemsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<FridgeViewModel.FridgeItem>().ToListAsync();
        }

        // save fridge items in the database
        public async Task<int> SaveFridgeItemAsync(FridgeViewModel.FridgeItem item)
        {
            await InitializeAsync();
            return item.Id != 0 ? 
                await _database!.UpdateAsync(item) : 
                await _database!.InsertAsync(item);
        }

        // remove fridge item 
        public async Task<int> DeleteFridgeItemAsync(FridgeViewModel.FridgeItem item)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(item);
        }
        #endregion



        #region Grocery Items
        
        // get all items from the grocery list database
        public async Task<List<GroceryViewModel.GroceryItem>> GetGroceryItemsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<GroceryViewModel.GroceryItem>().ToListAsync();
        }

        // save grocery items in the database
        public async Task<int> SaveGroceryItemAsync(GroceryViewModel.GroceryItem item)
        {
            await InitializeAsync();
            return item.Id != 0 ? 
                await _database!.UpdateAsync(item) : 
                await _database!.InsertAsync(item);
        }

        // remove grocery item
        public async Task<int> DeleteGroceryItemAsync(GroceryViewModel.GroceryItem item)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(item);
        }

        // delete enitre grocery item list
        public async Task ClearAllGroceryItems()
        {
            await InitializeAsync();
            await _database!.DeleteAllAsync<GroceryViewModel.GroceryItem>();
        }
        #endregion
    }

}
