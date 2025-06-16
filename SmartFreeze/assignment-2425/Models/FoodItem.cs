using System.Text.Json.Serialization;

namespace assignment_2425.Models
{

    //search results from food API
    public class FoodSearchResult
    {
        [JsonPropertyName("foods")]
        public List<FoodItem> Foods { get; set; } = new();
    }
    public class FoodItem
    {
        [JsonPropertyName("fdcId")]
        public int FdcId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("foodNutrients")]
        public List<FoodNutrient> FoodNutrients { get; set; } = new();
        [JsonPropertyName("brandName")]
        public string BrandName { get; set; } = string.Empty;

        public string DisplayName =>
            string.IsNullOrEmpty(BrandName) ?
                Description :
                $"{BrandName} - {Description}";

        public string PrimaryNutrients =>
            FoodNutrients
                .Where(n => new[] { "Protein", "Carbohydrate", "Fat", "Fiber", "Sugars" }
                .Contains(n.NutrientName) && n.Value > 0)
                .Select(n => $"{n.NutrientName}:{n.Value}{n.UnitName}")
                .DefaultIfEmpty("Nutrition info not available")
                .Aggregate((a, b) => $"{a}, {b}");
    }

    public class FoodNutrient
    {
        [JsonPropertyName("nutrientName")]
        public string NutrientName { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
        [JsonPropertyName("unitName")]

        public string UnitName { get; set; } = string.Empty;
    }

}
