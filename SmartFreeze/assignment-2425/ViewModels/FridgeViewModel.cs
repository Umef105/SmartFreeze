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
    // ViewModel for managing fridge contents and food items
    public class FridgeViewModel : INotifyPropertyChanged
    {
        // services
        private readonly IDatabaseService _databaseService;
        private readonly FoodDataService _foodService;
        private readonly ISpeechService _speechService;

        private string _searchQuery = string.Empty;
        private bool _isBusy;
        private FoodItem? _selectedFood;
        private int _newItemQuantity = 1;
        private DateTime _newItemExpiry = DateTime.Today.AddDays(7);

        // collections
        public ObservableCollection<FoodItem> SearchResults { get; } = new();
        public ObservableCollection<FridgeItem> FridgeItems { get; } = new();


        // Commands 
        public ICommand SearchCommand { get; }
        public ICommand AddToFridgeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoToAddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand SelectFoodCommand { get; }
        public ICommand LoadFridgeItemsCommand { get; }
        public ICommand ReadAloudCommand { get; }

        // properties 
        public DateTime Today => DateTime.Today;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();

            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
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
        public DateTime NewItemExpiry
        {
            get => _newItemExpiry;
            set => SetProperty(ref _newItemExpiry, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FridgeViewModel(FoodDataService foodService, IDatabaseService databaseService, ISpeechService speechService)
        {
            _foodService = foodService;
            _databaseService = databaseService;
            _speechService = speechService;

            // initialise commands 
            ReadAloudCommand = new Command(async () => await ReadAloud());
            LoadFridgeItemsCommand = new Command(async () => await LoadFridgeItems());
            SearchCommand = new Command(async () =>
            {
                await SearchFoods();
            });
            AddToFridgeCommand = new Command<FoodItem>(async (food) => await AddToFridge(food)); ;
            SaveCommand = new Command(async () => await SaveFridgeItems());
            GoToAddItemCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(FridgeAddItemView));
            });
            DeleteItemCommand = new Command<FridgeItem>(async (item) => await DeleteItem(item));
            SelectFoodCommand = new Command<FoodItem>(SelectFood);

            // initial load
            Task.Run(async () => await LoadFridgeItems());
        }

        // Searches for food items using the FoodDataService
        private async Task SearchFoods()
        {
            Debug.WriteLine($"Search initiated with query: '{SearchQuery}'");

            if (_isBusy || string.IsNullOrWhiteSpace(SearchQuery))
            {
                Debug.WriteLine("Search skipped - busy or empty query");
                return;
            }
            try
            {
                IsBusy = true;
                HapticFeedbackService.PerformHapticFeedback();
                SearchResults.Clear();
                Debug.WriteLine("Cleared search results");

                var result = await _foodService.SearchFoodsAsync(SearchQuery);
                Debug.WriteLine($"Received {result?.Foods?.Count ?? 0} results");

                if (result?.Foods != null)
                {
                    foreach (var food in result.Foods)
                    {
                        SearchResults.Add(food);
                    }
                    Debug.WriteLine($"Added {result.Foods.Count} items to SearchResults");
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

            finally { IsBusy = false; }
            Debug.WriteLine("Search completed");

        }

        // Load all fridge items from the database
        private async Task LoadFridgeItems()
        {
            try
            {
                IsBusy = true;
                var items = await _databaseService.GetFridgeItemsAsync();
                FridgeItems.Clear();
                foreach (var item in items)
                {
                    FridgeItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load items: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load fridge items", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Add a food item to the fridge
        private async Task AddToFridge(FoodItem food)
        {
            if (food == null) return;

            try {
                IsBusy = true;
                HapticFeedbackService.PerformHapticFeedback();

                var existingItem = FridgeItems.FirstOrDefault(f => f.Food.FdcId == food.FdcId);


                if (existingItem != null)
                {
                    existingItem.Quantity += NewItemQuantity;
                    existingItem.ExpiryDate = NewItemExpiry;
                    await _databaseService.SaveFridgeItemAsync(existingItem);
                }
                else
                {
                    // new item
                    var newItem = new FridgeItem(food)
                    {
                        Quantity = NewItemQuantity,
                        ExpiryDate = NewItemExpiry,
                    };
                    await _databaseService.SaveFridgeItemAsync(newItem);
                }

                await LoadFridgeItems();

                SelectedFood = null;
                SearchResults.Clear();
                SearchQuery = string.Empty;

                await Shell.Current.GoToAsync("//FridgePage");
            }
            catch (Exception ex) { 
                Debug.WriteLine($"Error adding to fridge: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to add item to fridge", "OK");
            }
            finally { IsBusy = false; }
            }


        // Deletes an item from the fridge
        private async Task DeleteItem(FridgeItem item)
        {
            try
            {
                if (item == null) return;

                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirm Delete",
                    $"Delete {item.Food.Description} from your fridge?",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    if (Vibration.Default.IsSupported)
                    {
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                    }
                    await _databaseService.DeleteFridgeItemAsync(item);
                    await LoadFridgeItems();
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

        // Saves all current fridge items to the database
        private async Task SaveFridgeItems()
        {
            try
            {
                IsBusy = true;
                HapticFeedbackService.PerformHapticFeedback();

                foreach (var item in FridgeItems)
                {
                    await _databaseService.SaveFridgeItemAsync(item);
                }

                Debug.WriteLine($"Saved {FridgeItems.Count} items to fridge");
                await Shell.Current.DisplayAlert("Success", "Fridge items saved", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving items: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to save fridge items", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Reads fridge contents aloud using text-to-speech
        public async Task ReadAloud()
        {
            var title = "Your Fridge Contents";
            var content = new System.Text.StringBuilder();

            if (FridgeItems.Any())
            {
                content.AppendLine($"Contains {FridgeItems.Count} items:");

                foreach (var item in FridgeItems)
                {
                    var hasBrand = !string.IsNullOrWhiteSpace(item.BrandName);
                    var description = hasBrand
                        ? $"{item.BrandName} {item.Description}"
                        : item.Description;
                    var expiryStatus = item.ExpiryDate.HasValue ?
                        $"expires {item.ExpiryDate:MMMM d}" :
                        "no expiry date";

                    content.AppendLine(
                        $"{item.Quantity} {description}, {expiryStatus}, {item.ExpiryPriority}. ");
                }
            }
            else
            {
                content.Append("Your fridge is empty.");
            }

            await _speechService.SpeakPageContentAsync(title, content.ToString());
        }

        // helper methods 
        private void SelectFood(FoodItem food)
        {
            SelectedFood = food;
            NewItemQuantity = 1;
            NewItemExpiry = DateTime.Today.AddDays(7);
        }

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

        // Represents an item in the fridge items list
        public class FridgeItem
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public int FdcId { get; set; }
            [Ignore]
            public FoodItem Food { get; set; } = new();
            public string BrandName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string PrimaryNutrients { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public DateTime AddedDate { get; set; } = DateTime.Now;
            public DateTime? ExpiryDate { get; set; }

            public FridgeItem() { }
            public FridgeItem(FoodItem food)
            {
                Food = food;
                FdcId = food.FdcId;
                BrandName = food.BrandName;
                Description = food.Description;
                PrimaryNutrients = food.PrimaryNutrients;
            }

            public string ExpiryStatus =>
                ExpiryDate.HasValue ?
                    (ExpiryDate.Value < DateTime.Now ? "Expired" :
                    ExpiryDate.Value < DateTime.Now.AddDays(2) ? "Urgent (0-1 days)" :
                    ExpiryDate.Value < DateTime.Now.AddDays(4) ? "Soon (2-3 days)" : "Good (4+ days)") :
                    "Not Set";

            // Text description of expiry status
            public string ExpiryPriority
            {
                get
                {
                    if (!ExpiryDate.HasValue) return "No expiry date";

                    var daysUntilExpiry = (ExpiryDate.Value - DateTime.Today).Days;

                    return daysUntilExpiry switch
                    {
                        < 0 => "Expired",
                        < 2 => "Urgent (0-1 days)",
                        < 4 => "Soon (2-3 days)",
                        _ => "Good (4+ days)"
                    };
                }
            }

            // Color representing expiry urgency
            public Color StatusColor
            {
                get
                {
                    if (!ExpiryDate.HasValue) return Colors.Gray;

                    var daysUntilExpiry = (ExpiryDate.Value - DateTime.Today).Days;

                    return daysUntilExpiry switch
                    {
                        < 0 => Colors.Red,
                        < 2 => Colors.Orange,
                        < 4 => Colors.Yellow,
                        _ => Colors.Green
                    };
                }
            }
        }

    }
}
