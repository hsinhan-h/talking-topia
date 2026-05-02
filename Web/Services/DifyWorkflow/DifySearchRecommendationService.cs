using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Web.Services.DifyWorkflow.Dtos;

namespace Web.Services.DifyWorkflow
{
    public class DifySearchRecommendationService
    {
        private readonly string _difyApiUrl;
        private readonly string _difySearchRecommendationApiKey;
        private readonly IHttpClientFactory _httpClientFactory;

        public DifySearchRecommendationService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _difyApiUrl = configuration["DifyWorkFlowApiEndpoint"];
            _difySearchRecommendationApiKey = configuration["DifySearchRecommendationApiKey"];
            _httpClientFactory = httpClientFactory;
        }
        public async Task<DifyWorkflowResponse> CreateSearchRecommendation(CreateSearchRecommendationRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _difySearchRecommendationApiKey);

            var endpoint = $"{_difyApiUrl}/workflows/run";
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var runWorkflowResponse = JsonSerializer.Deserialize<DifyWorkflowResponse>(result);
                return runWorkflowResponse;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error running workflow: {errorResponse}");
            }
        }

        public string ProcessResponse(string jsonOutput)
        {
            // 將 JSON 字符串反序列化為 Dictionary
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonOutput);

            if (dictionary != null && dictionary.ContainsKey("category_name"))
            {
                // 取得 category_name 的值
                string categoryName = dictionary["category_name"];

                // 如果需要將 Unicode 轉為正常字符，可以使用 Unescape
                categoryName = System.Text.RegularExpressions.Regex.Unescape(categoryName);

                // 打印或進行後續處理
                Console.WriteLine($"Category Name: {categoryName}");

                return categoryName;
            }
            else
            {
                return "處理失敗，找不到這個臭玩意兒";
            }
        }
    }
}
