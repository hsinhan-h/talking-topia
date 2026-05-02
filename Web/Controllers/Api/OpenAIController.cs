using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Web.Dtos.ReviewCheckedDto;

namespace Web.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;

        public OpenAIController(IOpenAIService openAIService)
        {
           _openAIService = openAIService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContent([FromBody]string userContent)
        {
            // 自定義關鍵詞檢測
            string[] inappropriateWords = { "fuck", "shit", "damn","幹你娘","操","幹","幹你娘機掰" };
            foreach (var word in inappropriateWords)
            {
                if (userContent.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { success = false, message = "你的評論包含不當詞語，請修正後才可提交。" });
                }
            }

            var moderationResultJson = await _openAIService.ModerateContentAsync(userContent);
            // 將 JSON 字符串解析為 ModerationResult 類型
            var moderationResult = JsonConvert.DeserializeObject<ModerationResult>(moderationResultJson);
            // 判斷 results 陣列中的第一個項目的 flagged 屬性
            if (moderationResult.Results[0].Flagged )
            {
                return Ok(new { success = false, message = "你的評論包含不良內容，請修正後才可提交。" });
            }

            // 處理成功提交的內容
            return Ok(new { success = true });
        }
    }
}
