using System.Text.Json.Serialization;

namespace Web.Services.OpenAI.Dtos
{
    public class FilterResult
    {
        [JsonPropertyName("category_name")] public string CategoryName { get; set; }
        [JsonPropertyName("max_price")] public decimal MaxPrice { get; set; }
        [JsonPropertyName("min_price")] public decimal MinPrice { get; set; }
        [JsonPropertyName("district")] public string District { get; set; }
    }
}
