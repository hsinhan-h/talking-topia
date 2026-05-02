using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web
{
    public class Dify
    {
        public static async Task<ChatMessagesResponse> CallDifyChatMessagesAPIAsync(string apiKey, object requestData)
        {
            var client = new HttpClient();

            // 設定 API 網址
            var apiUrl = $"https://agent.build-school.com/v1/chat-messages";

            // 設定 HTTP request headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // 將 requestData 物件序列化成 JSON 字串
            string jsonRequestData = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            // 建立 HTTP request 內容
            var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");
            // 傳送 HTTP POST request
            var response = await client.PostAsync(apiUrl, content);
            // 取得 HTTP response 內容
            var resultString = new StringBuilder();
            string difyConversationId = string.Empty;

            //檢查response是否成功
            if (response.IsSuccessStatusCode)
            {
                //如果成功的話，讀取回應的streaming
                var stream = await response.Content.ReadAsStreamAsync();
                //用StreamReader 一行一行讀取
                using (var reader = new StreamReader(stream))
                {
                    // 迴圈讀取每一行內容，直到文件結束
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        // 檢查內容：不要是空白 而且 以 "data:" 開頭
                        if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data:"))
                        {
                            // 把前綴字"data:"移除, 取得json內容
                            var json = line.Substring(5); // Remove "data:" prefix
                            // use Newtonsoft json to deserialize (將 JSON 字串反序列化為 ChunkChatCompletionResponse 物件)
                            var chunk = JsonSerializer.Deserialize<ChunkChatCompletionResponse>(json);
                            // 將回應中的 Answer 加到結果的字串中
                            resultString.Append(chunk.Answer);
                            // 如果 difyConversationId 是空的(還沒設定)，則將取得的 ConversationId 設定為 difyConversationId
                            if (difyConversationId == string.Empty)
                            {
                                difyConversationId = chunk.ConversationId;
                            }
                            Console.Write(chunk.Answer);
                        }
                    }
                }
                // 回傳包含ConversationId、Message、DifyConversationId 的 ChatMessagesResponse 物件
                return new ChatMessagesResponse()
                {
                    ConversationId = difyConversationId,
                    Message = resultString.ToString()
                };
            }
            else
            {
                // 如果回應不成功，則拋出 HttpRequestException
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error from API: {errorResponse}");
            }
        }
    }
    public class ChatMessagesResponse
    {
        public string ConversationId { get; set; }
        public string Message { get; set; }
    }

    public class ChunkChatCompletionResponse
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("task_id")]
        public string TaskId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("answer")]

        public string Answer { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }

        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; }

        [JsonPropertyName("audio")]
        public string Audio { get; set; }

        [JsonPropertyName("thought")]
        public string Thought { get; set; }

        [JsonPropertyName("observation")]
        public string Observation { get; set; }

        [JsonPropertyName("tool")]
        public string Tool { get; set; }

        [JsonPropertyName("tool_input")]
        public Dictionary<string, object> ToolInput { get; set; }

        [JsonPropertyName("file_id")]
        public string FileId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("belongs_to")]
        public string BelongsTo { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("metadata")]
        public object Metadata { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }

        [JsonPropertyName("retriever_resources")]
        public List<RetrieverResource> RetrieverResources { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class RetrieverResource
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("citation")]
        public string Citation { get; set; }
    }
}
