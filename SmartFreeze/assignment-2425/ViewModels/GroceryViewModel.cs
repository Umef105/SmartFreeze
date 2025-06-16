using System.ComponentModel;
using assignment_2425.Services;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using assignment_2425.Models;
using System.Diagnostics;
using System.Windows.Input;
using SQLite;
using assignment_2425.Views;

namespace assignment_2425.ViewModels
{
    // ViewModel for managing grocery list
    public class GroceryViewModel : INotifyPropertyChanged, IDisposable
    {
        // services 
        private readonly IDatabaseService _databaseService;
        private readonly FoodDataService _foodService;
        private readonly IAccelerometer _accelerometer;
        private readonly ISpeechService _speechService;


        private string _searchQuery = string.Empty;
        private bool _isBusy;
        private FoodItem? _selectedFood;
        private int _newItemQuantity = 1;
        private DateTime _lastShakeTime;

        // collections
        private ObservableCollection<GroceryItem> _groceryItems = new();
        public ObservableCollection<FoodItem> SearchResults { get; } = new();
        public ObservableCollection<GroceryItem> GroceryItems
        {
            get => _groceryItems;
            set
            {
                _groceryItems = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand AddToGroceryCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoToAddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand SelectFoodCommand { get; }
        public ICommand LoadGroceryItemsCommand { get; }
        public ICommand ReadAloudCommand { get; }


        // properties 
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public FoodItem? SelectedFood
        {
            get => _selectedFood;
            set => SetProperty(ref _selectedFood, value);
        }

        public int NewItemQuantity
        {
            get => _newItemQuantity;
            set => SetProperty(ref _newItemQuantity, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public GroceryViewModel(FoodDataService foodService, IDatabaseService databaseService, ISpeechService speechService)
        {
            _foodService = foodService;
            _databaseService = databaseService;
            _speechService = speechService;
            _accelerometer = Accelerometer.Default;

            // initialise commands 
            SearchCommand = new Command(async () => await SearchFoods());
            AddToGroceryCommand = new Command<FoodItem>(AddToGrocery);
            SaveCommand = new Command(async () => await SaveGroceryItems());
            GoToAddItemCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(GroceryAddItemView)));
            DeleteItemCommand = new Command<GroceryItem>(async (item) => await DeleteItem(item));
            SelectFoodCommand = new Command<FoodItem>(SelectFood);
            ReadAloudCommand = new Command(async () => await ReadAloud());
            LoadGroceryItemsCommand = new Command(async () => await LoadGroceryItems());

            InitialiseShakeDetection();
        }

        // Search for food items using the FoodDataService
        private async Task SearchFoods()
        {
            if (_isBusy || string.IsNullOrWhiteSpace(SearchQuery))
                return;

            try
            {
                IsBusy = true;
                HapticFeedbackService.PerformHapticFeedback();
                SearchResults.Clear();

                var result = await _foodService.SearchFoodsAsync(SearchQuery);

                if (result?.Foods != null)
                {
                    foreach (var food in result.Foods)
                    {
                        SearchResults.Add(food);
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Info", "No results found", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error",
                    $"Failed to search: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Adds a food item to the grocery list
        private async void AddToGrocery(FoodItem food)
        {
            if (food == null) return;

            try
            {
                IsBusy = true;
                HapticFeedbackService.PerformHapticFeedback();

                // Check if item already exists in list
                var existingItem = GroceryItems.FirstOrDefault(f => f.Food.FdcId == food.FdcId);

                if (existingItem != null)
                {
                    // Update existing item quantity
                    existingItem.Quantity += NewItemQuantity;
                    await _databaseService.SaveGroceryItemAsync(existingItem);
                }
                else
                {
                    // new item
                    var newItem = new GroceryItem
                    {
                        Food = food,
                        Quantity = NewItemQuantity,
                        BrandName = food.BrandName ?? string.Empty,
                        Description = food.Description ?? string.Empty,
                        AddedDate = DateTime.Now
                    };
                     GroceryItems.Add(newItem);
                     await _databaseService.SaveGroceryItemAsync(newItem);
                }

                SelectedFood = null;
                SearchResults.Clear();
                SearchQuery = string.Empty;

                await Shell.Current.GoToAsync("//GroceryPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to grocery: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to add item", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Loads all grocery items from the database
        private async Task LoadGroceryItems()
        {
            GroceryItems.Clear();
            var items = await _databaseService.GetGroceryItemsAsync();
            foreach (var item in items)
            GroceryItems.Add(item);
        }

        // Deletes an item from the grocery list
        private async Task DeleteItem(GroceryItem item)
        {
            try
            {
                if (item == null) return;

                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirm Delete",
                    $"Delete {item.Food.Description} from your grocery list?",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    if (Vibration.Default.IsSupported)
                    {
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                    }
                    GroceryItems.Remove(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting item: {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"Failed to delete item: {ex.Message}",
                    "OK");
            }
        }

        // Saves all grocery items

        private async Task SaveGroceryItems()
        {
            try
            {
                IsBusy = true;

                foreach (var item in GroceryItems)
                {
                    await _databaseService.SaveGroceryItemAsync(item);
                }

                Debug.WriteLine($"Saved {GroceryItems.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save failed: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to save items", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // Sets the selected food and quantity
        private void SelectFood(FoodItem food)
        {
            SelectedFood = food;
            NewItemQuantity = 1;
        }

        // Initialises shake detection for clear-all functionality
        private void InitialiseShakeDetection()
        {
            if (_accelerometer.IsSupported)
            {
                _accelerometer.ReadingChanged += OnShakeDetected;
                _accelerometer.Start(SensorSpeed.Game);
            }
        }

        // Handles device shake events to trigger grocery list clearing
        private void OnShakeDetected(object? sender, AccelerometerChangedEventArgs e)
        {
            var accel = e.Reading.Acceleration;

            // Detect significant shake 
            if (Math.Abs(accel.X) > 3 || Math.Abs(accel.Y) > 3 || Math.Abs(accel.Z) > 3)
            {
                // ignore shakes within 2 seconds
                if ((DateTime.Now - _lastShakeTime).TotalSeconds < 2)
                    return;

                _lastShakeTime = DateTime.Now;

                MainThread.BeginInvokeOnMainThread(async() =>
                {
                    var mainPage = Application.Current?.MainPage;

                    if (mainPage != null)
                    {
                        // Confirm with user before clearing
                        bool confirm = await mainPage.DisplayAlert(
                            "Clear List",
                            "Shake detected! Clear all grocery items?",
                            "Yes", "No");

                        if (confirm)
                        {
                            await ClearAllGroceryItems();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("MainPage is null. Cannot show alert.");
                    }

                });
            }
        }

        // Clears all items from the grocery list
        private async Task ClearAllGroceryItems()
        {
            try
            {
                await _databaseService.ClearAllGroceryItems();
                GroceryItems.Clear();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"Failed to clear items: {ex.Message}",
                    "OK");
            }
        }

        // Reads grocery list contents aloud using text-to-speech
        public async Task ReadAloud()
        {
            var title = "Your Grocery List";
            var itemDetails = new System.Text.StringBuilder();

            if (GroceryItems.Any())
            {
                itemDetails.AppendLine($"Contains {GroceryItems.Count} items:");

                foreach (var item in GroceryItems)
                {
                    var actualDescription = !string.IsNullOrWhiteSpace(item.Description)
                        ? item.Description
                        : item.Food?.Description ?? "unnamed item";

                    var hasBrand = !string.IsNullOrWhiteSpace(item.BrandName);
                    var fullDescription = hasBrand
                        ? $"{item.BrandName} {actualDescription}"
                        : actualDescription;

                    itemDetails.AppendLine($"{item.Quantity} {fullDescription}.");
                }
            }
            else
            {
                itemDetails.Append("The list is currently empty.");
            }

            await _speechService.SpeakPageContentAsync(title, itemDetails.ToString());
        }

        // Cleans up accelerometer resources
        public void Dispose()
        {
            if (_accelerometer.IsSupported && _accelerometer.IsMonitoring)
            {
                _accelerometer.Stop();
                _accelerometer.ReadingChanged -= OnShakeDetected;
            }
        }

        // helper method
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Represents an item in the grocery list
        public class GroceryItem
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            [Ignore]
            public FoodItem Food { get; set; } = new();
            public string BrandName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public DateTime AddedDate { get; set; } = DateTime.Now;
        }

    }
}
