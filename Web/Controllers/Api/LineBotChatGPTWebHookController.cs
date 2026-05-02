using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers.Api
{
    public class LineBotChatGPTWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        private readonly string _difyAPIKey;
        private readonly string _adminUserId;
        private readonly string _channelAccessToken;
        private readonly CacheService _cacheService;

        public LineBotChatGPTWebHookController(IConfiguration configuration, CacheService cacheService)
        {
            _difyAPIKey = configuration["DifyAI:DifyAPIKey"];
            _adminUserId = configuration["DifyAI:AdminUserId"];
            _channelAccessToken = configuration["DifyAI:ChannelAccessToken"];
            _cacheService = cacheService;
        }

        [Route("api/Dify2LineBotWebHook")]
        [HttpPost]
        public async Task<IActionResult> POST()
        {

            // 如果有需要，可以透過 QueryString 傳入 DifyAPIKey, adminUserId, channelAccessToken
            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = _channelAccessToken;
                //配合Line Verify
                if (ReceivedMessage.events == null || ReceivedMessage.events.Count() <= 0 ||
                    ReceivedMessage.events.FirstOrDefault().replyToken == "00000000000000000000000000000000") return Ok();
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();

                var responseMsg = "";
                //準備回覆訊息
                if (LineEvent != null && LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {
                    //取得對話ID(from cache)
                    var Conversation_id = _cacheService.GetCache(LineEvent.source.userId);

                    //如果用戶輸入 /forget 則把 Conversation_id 清空，重啟對話
                    if (LineEvent.message.text.Trim().ToLower() == "/forget")
                    {
                        _cacheService.RemoveCache(LineEvent.source.userId);
                        Conversation_id = null;
                        responseMsg = "我已經忘記之前所有對話了";
                    }
                    else
                    {
                        //👇建立呼叫 Dify API 所需的 requestData 參數
                        var requestData = new
                        {
                            inputs = new { },
                            query = LineEvent.message.text, //👉取得使用者輸入文字
                            response_mode = "streaming",
                            conversation_id = string.IsNullOrEmpty(Conversation_id) ? "" : Conversation_id.ToString(), //👉取得對話ID
                            user = LineEvent.source.userId //👉取得使用者ID
                        };
                        var response = await Dify.CallDifyChatMessagesAPIAsync(_difyAPIKey, requestData);
                        responseMsg = response.Message;
                        //儲存對話ID(to cache)
                        _cacheService.SetCache(LineEvent.source.userId, response.ConversationId);
                    }
                }
                else if (LineEvent != null && LineEvent.type.ToLower() == "message")
                    responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
                else
                    responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, responseMsg);
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {

                //回覆訊息
                this.PushMessage(_adminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }
}
