using System.Text.Json;

namespace Web.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DifyController : ControllerBase
    {
        //private readonly string _difyUserId;
        //private readonly ILogger<DifyController> _logger;
        //private readonly DifySearchService _difySearchService;

        //public DifyController(IConfiguration configuration,
        //                  ILogger<DifyController> logger,
        //                  DifySearchService difySearchService)
        //{
        //    _difyUserId = configuration["DifyUserId"];
        //    _logger = logger;
        //    _difySearchService = difySearchService;
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateSearchRecommendation([FromBody] CreateWorkflowRequest request)
        //{
        //    var inputs = new Dictionary<string, object>();
        //    inputs.Add("input", request.ProductName);
        //    var runWorkflowRequest = new CreateSearchRecommendationRequest
        //    {
        //        Inputs = inputs,
        //        ResponseMode = "blocking",
        //        User = _difyUserId
        //    };
        //    try
        //    {
        //        var response = await _difySearchService.CreateSearch(runWorkflowRequest);
        //        var apiResponse = new ApiResponse
        //        {
        //            IsSuccess = true,
        //            Code = ApiStatusCode.Success,
        //            Body = response.Data.Outputs
        //        };


        //        string jsonString = apiResponse.Body.ToString();
        //        string result = _difySearchService.ProcessResponse(jsonString);

        //        Console.WriteLine(result);

        //        //呼叫方法傳出去做資料表搜尋

        //        return Ok(apiResponse);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "An error occurred while creating the workflow.");
        //        var apiResponse = new ApiResponse
        //        {
        //            IsSuccess = false,
        //            Code = ApiStatusCode.Error,
        //            Body = "An error occurred while processing your request."
        //        };
        //        return Ok(apiResponse);
        //    }
        //}

        private readonly string _difyUserId;
        private readonly ILogger<DifyController> _logger;
        private readonly DifySearchRecommendationService _difySearchRecommendationService;

        public DifyController(IConfiguration configuration,
                              ILogger<DifyController> logger,
                              DifySearchRecommendationService difySearchRecommendationService)
        {
            _difyUserId = configuration["DifyUserId"];
            _logger = logger;
            _difySearchRecommendationService = difySearchRecommendationService;
        }

        public async Task<IActionResult> CreateSearchRecommendation([FromBody] CreateWorkflowRequest request)
        {
            if (string.IsNullOrEmpty(request.ProductName))
            {
                return BadRequest("ProductName cannot be null or empty.");
            }

            var inputs = new Dictionary<string, object>
    {
        { "input", request.ProductName }
    };
            var runWorkflowRequest = new CreateSearchRecommendationRequest
            {
                Inputs = inputs,
                ResponseMode = "blocking",
                User = _difyUserId
            };
            try
            {
                var response = await _difySearchRecommendationService.CreateSearchRecommendation(runWorkflowRequest);
                var apiResponse = new ApiResponse
                {
                    IsSuccess = true,
                    Code = ApiStatusCode.Success,
                    Body = response.Data.Outputs
                };

                string jsonString = JsonSerializer.Serialize(apiResponse.Body);
                string result = _difySearchRecommendationService.ProcessResponse(jsonString);

                // Enum values for validation
                var enumValues = new List<string> { "語言學習", "程式設計", "升學補習", "英文", "日文", "中文", "德文", "法文", "西班牙文", "HTML/CSS", "JavaScript", "C#", "SQL", "Python", "Java", "數學", "物理", "化學", "歷史", "地理", "生物", "English", "Japanese", "Spanish", "Math", "History" };

                if (!enumValues.Contains(result))
                {
                    _logger.LogWarning($"Unknown category returned: {result}");
                    result = "未知類別";
                }

                Console.WriteLine(result);

                return Ok(apiResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while creating the workflow.");
                var apiResponse = new ApiResponse
                {
                    IsSuccess = false,
                    Code = ApiStatusCode.Error,
                    Body = "An error occurred while processing your request."
                };
                return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            }
        }
    }
}
