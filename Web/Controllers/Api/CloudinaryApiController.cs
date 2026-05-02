using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudinaryApiController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public CloudinaryApiController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                //ViewBag.ImageUrl = imageUrl;
            }
            return Ok();
        }
        /// <summary>
        /// 給課程上傳圖片的API
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        [HttpPost("UploadImages")]
        public async Task<IActionResult> UploadImages(List<IFormFile> images)
        {
            if (images == null || !images.Any())
            {
                return BadRequest("沒有圖片");
            }

            var imagePaths = new List<string>();
            foreach (var image in images)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                imagePaths.Add(imageUrl); // 保存圖片路徑
            }

            return Ok(imagePaths); // 回傳圖片的檔案名
        }
    }
}
