using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Web.Services;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CourseService _courseService;
        private readonly MemberDataService _memberDataService;
        private readonly IRepository _repository;
        private readonly CourseCategoryService _courseCategoryService;


        public HomeController(ILogger<HomeController> logger, CourseService courseService, MemberDataService memberDataService, IRepository repository, CourseCategoryService courseCategoryService)
        {
            _logger = logger;
            _courseService = courseService;
            _memberDataService = memberDataService;
            _repository = repository;
            _courseCategoryService = courseCategoryService;
        }

        public async Task<IActionResult> Index()
        {

            // 預設加載「語言學習」的資料
            var defaultCategory = "語言學習";
            var courses = await _courseCategoryService.GetCoursesByCategoryAsync(defaultCategory);

            // 將資料傳遞給 View
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesByCategory(string categoryName)
        {
            var courses = await _courseCategoryService.GetCoursesByCategoryAsync(categoryName);

            if (courses == null || courses.Count == 0)
            {
                Console.WriteLine("No data found for category: " + categoryName);
                return NotFound();  // 如果找不到數據，返回 404
            }

            Console.WriteLine($"Data for {categoryName}: {JsonConvert.SerializeObject(courses)}");

            return Json(courses);  // 返回 JSON 資料
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Questions()
        {
            return View();
        }
    }
}
