using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text.Json;
using Web.Services.OpenAI.Dtos;


namespace Web.Services.OpenAI
{
    public class OpenAIService
    {
        private readonly string _apiKey;

        // 同義詞映射字典
        private readonly Dictionary<string, string> _categorySynonyms = new Dictionary<string, string>
    {
        { "Japanese", "日文" },
        { "English", "英文" },
        { "Spanish", "西班牙文" },
        { "English", "數學" },
        { "History", "歷史" },
        // 可以繼續添加更多同義詞
    };

        public OpenAIService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAIApiKey"];
        }

        public async Task<FilterResult> GetFilterResultAsync(string prompt)
        {
            ChatClient client = new ChatClient("gpt-4o-mini", _apiKey);

            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "course_assistant_prompts",
                    jsonSchema: BinaryData.FromString("""
                                                  {
                                                    "type": "object",
                                                    "properties": {
                                                      "category_name": {
                                                        "type": "string",
                                                        "enum":["語言學習","程式設計","升學補習","英文","日文","中文","德文","法文","西班牙文","HTML/CSS","JavaScript","C#","SQL","Python","Java","數學","物理","化學","歷史","地理","生物","English","Japanese","Spanish","Math","History"]
                                                      },
                                                      "max_price": {
                                                        "type": "number",
                                                        "description": "The maximum price of the course that evaluated."
                                                      },
                                                      "min_price": {
                                                        "type": "number",
                                                        "description": "The minimum price of the course that evaluated."
                                                      },
                                                      "district": {
                                                        "type": "string",
                                                        "description": "The district of the course package that must be in Taiwan."
                                                      }
                                                    },
                                                    "required": ["category_name", "max_price", "min_price", "district"],
                                                    "additionalProperties": false
                                                  }
                                                  """),
                    jsonSchemaIsStrict: true)
            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage(
                "您是專業的課程推薦專家，能夠根據用戶的需求和喜好提供量身定制的課程計劃和建議。從給定文本中提取類別名稱、您評估的最高價格、您評估的最低價格和區域信息"),
            new UserChatMessage(prompt),
        ];

            ChatCompletion chatCompletion = await client.CompleteChatAsync(messages, options);
            var chatCompletionText = chatCompletion.Content[0].Text;
            var result = JsonSerializer.Deserialize<FilterResult>(chatCompletionText);
            return result;
        }
    }
}
