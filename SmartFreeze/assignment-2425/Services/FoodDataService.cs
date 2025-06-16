using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using assignment_2425.Models;


namespace assignment_2425.Services
{
    public class FoodDataService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private const string ApiKey = "sjalm01tRpYHcbk6KtXm1fcbIovz2ZJx1NjbY42C";
        private const string Url = "https://api.nal.usda.gov/fdc/v1";
        public FoodDataService()
        {
            _httpClient = new HttpClient();
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        // search for foods using USDA API
        public async Task<FoodSearchResult> SearchFoodsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new ArgumentException("Search query cannot be empty");
                }

                var response = await _httpClient.GetAsync(
            $"{Url}/foods/search?api_key={ApiKey}&query={Uri.EscapeDataString(query)}&pageSize=10");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"API Response: {content}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    return JsonSerializer.Deserialize<FoodSearchResult>(content, options)
                        ?? new FoodSearchResult { Foods = new List<FoodItem>() };
                }
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new FoodSearchResult { Foods = new List<FoodItem>() };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Network Error: {ex:Message}");
                return new FoodSearchResult { Foods = new List<FoodItem>()};
            }
        }

        // container for API search results
        public class FoodSearchResult
        {
            [JsonPropertyName("foods")]
            public List<FoodItem> Foods { get; set; } = new List<FoodItem>();
        }
    }
}
