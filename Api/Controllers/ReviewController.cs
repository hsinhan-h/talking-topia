using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewApiService _reviewApiService;

        public ReviewController(ReviewApiService reviewApiService)
        {
            _reviewApiService = reviewApiService;
        }

        /// <summary>
        /// 撈取評論
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var result = await _reviewApiService.GetAllReviews();

            return Ok(new BaseApiResponse(result));
        }

        /// <summary>
        /// 刪除評論
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                await _reviewApiService.DeleteReview(reviewId);
                return Ok(new BaseApiResponse { ErrMsg = "Reviews deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseApiResponse { ErrMsg = "An error occurred while deleting the review." });
            }
        }

        /// <summary>
        /// 刪除多筆評論
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteReviews(List<int> reviewIds)
        {
            if (reviewIds == null || !reviewIds.Any())
            {
                return BadRequest(new { error = "No review IDs provided." });
            }

            try
            {
                await _reviewApiService.DeleteReviews(reviewIds);

                return Ok(new BaseApiResponse { ErrMsg = "Reviews deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseApiResponse{ ErrMsg = "An error occurred while deleting the reviews."});
            }
        }
    }
}
