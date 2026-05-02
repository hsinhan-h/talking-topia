using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string query)
        {
            //if (string.IsNullOrEmpty(query))
            //{
            //    // 如果查詢為空，返回 CourseList 視圖
            //    var allCourses = await _searchService.SearchCoursesAsync("");
            //    return View("CourseList", allCourses);
            //}

            //var results = await _searchService.SearchCoursesAsync(query);

            //if (results.Any())
            //{
            //    // 進行重定向到 CourseList
            //    return RedirectToAction("CourseList", "Course", new { subject = query });
            //}
            //else
            //{
            //    // 處理無結果的情況
            //    ViewData["Message"] = "無資料";
            //    return View("NoResults"); // 確保有一個 NoResults.cshtml
            //}
            if (string.IsNullOrEmpty(query))
            {
                // 返回空結果的 JSON
                return Json(new { success = false, message = "查詢為空" });
            }

            var results = await _searchService.SearchCoursesAsync(query);

            if (results.Any())
            {
                // 有結果時返回成功信息
                return Json(new { success = true, redirectUrl = Url.Action("CourseList", "Course", new { subject = query }) });
            }
            else
            {
                // 無結果時返回信息
                return Json(new { success = false, message = "無資料" });
            }
        }
    }
}
