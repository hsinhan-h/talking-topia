using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class CloudinaryController : Controller
    {
        private readonly CloudinaryService _cloudinaryService;

        public CloudinaryController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
        /// <summary>
        /// 給上傳圖片測試使用(有View畫面)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                ViewBag.ImageUrl = imageUrl;
            }
            return View();
        }
    }
}
