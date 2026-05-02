using ApplicationCore.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly string _apiKey = "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";
        private readonly HttpClient _httpClient;

        public OpenAIService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> ModerateContentAsync(string inputText)
        {
            var url = "https://api.openai.com/v1/moderations";
            var requestBody = new
            {
                input = inputText
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            else
            {
                // 處理錯誤狀況
                return $"Error: {response.StatusCode}";
            }
        }
    }
}
