using Microsoft.AspNetCore.Mvc;
using Web.Services.OpenAI;

namespace Web.Controllers.Api
{
    public class SearchFilterController : ControllerBase
    {
        private readonly OpenAIService _openAIService;
        private readonly ILogger<SearchFilterController> _logger;

        public SearchFilterController(OpenAIService openAiService, ILogger<SearchFilterController> logger)
        {
            _openAIService = openAiService;
            _logger = logger;
        }

        public async Task<IActionResult> GetFilterResult([FromQuery] string prompt)
        {
            try
            {
                var response = await _openAIService.GetFilterResultAsync(prompt);
                var apiResponse = new ApiResponse
                {
                    IsSuccess = true,
                    Code = ApiStatusCode.Success,
                    Body = response
                };
                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while processing the open ai api request.");
                var apiResponse = new ApiResponse
                {
                    IsSuccess = false,
                    Code = ApiStatusCode.Error,
                    Body = "An error occurred while processing open ai api request."
                };
                return Ok(apiResponse);
            }
        }
    }
}
