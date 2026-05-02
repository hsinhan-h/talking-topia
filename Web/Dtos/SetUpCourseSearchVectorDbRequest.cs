using System.Text.Json.Serialization;

namespace Web.Dtos
{
    public class SetUpCourseSearchVectorDbRequest
    {
        [JsonPropertyName("start_index")] public int StartIndex { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
    }
}
