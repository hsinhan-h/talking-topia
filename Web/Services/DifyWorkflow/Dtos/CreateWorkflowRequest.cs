using System.Text.Json.Serialization;

namespace Web.Services.DifyWorkflow.Dtos
{
    public class CreateWorkflowRequest
    {
        [JsonPropertyName("product_name")]
        [Required]
        [Length(2, 30)]
        public string ProductName { get; set; }
    }
}
