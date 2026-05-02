using ApplicationCore.Entities;
using Web.Helpers;

namespace Web.Services
{
    public class CourseCategoryService
    {
        private readonly IRepository _repository;
        private readonly RedisCacheHelper _cacheHelper;

        public CourseCategoryService(IRepository repository, RedisCacheHelper cacheHelper)
        {
            _repository = repository;
            _cacheHelper = cacheHelper;
        }

        public async Task<List<CourseCategoryViewModel>> GetCourseCategoriesWithSubjectsAsync()
        {
            //如果redis cache已有類別科目清單資料, 直接return cache資料
            var cacheKey = "CourseCategoriesWithSubjects";
            var cachedData = await _cacheHelper.GetFromCacheAsync<List<CourseCategoryViewModel>>(cacheKey);
            if (cachedData != null)
                return cachedData;

            var courseCategoriesWithSubjects = await (
                from category in _repository.GetAll<Entities.CourseCategory>()
                join subject in _repository.GetAll<Entities.CourseSubject>()
                on category.CourseCategoryId equals subject.CourseCategoryId into subjectGroup
                select new CourseCategoryViewModel
                {
                    CategoryName = category.CategorytName,
                    Subjects = subjectGroup.Select(s => new CourseSubjectViewModel
                    {
                        SubjectName = s.SubjectName
                    }).ToList()
                }).ToListAsync();

            //將類別及科目清單查詢結果存到redis cache, SlidingExpiration設為30分, AbsoluteExpirationRelativeToNow設為1小時
            await _cacheHelper.SetCacheAsync(cacheKey, courseCategoriesWithSubjects, TimeSpan.FromMinutes(30), TimeSpan.FromHours(1));

            return courseCategoriesWithSubjects;
        }

        public async Task<CourseCategoryListViewModel> GetCourseCategoryListAsync()
        {
            var courseCategory =
                (from category in _repository.GetAll<Web.Entities.CourseCategory>()
                 select new CourseCategoryVM
                 {
                     CourseCategoryId = category.CourseCategoryId,
                     CategoryName = category.CategorytName,
                 });

            return new CourseCategoryListViewModel
            {
                CourseCategoryList = await courseCategory.ToListAsync(),
            };
        }


        public async Task<List<CourseTopicTabViewModel>> GetCoursesByCategoryAsync(string categoryName)
        {
            var courses = await (
                from category in _repository.GetAll<Entities.CourseCategory>()
                join subject in _repository.GetAll<Entities.CourseSubject>() on category.CourseCategoryId equals subject.CourseCategoryId
                join course in _repository.GetAll<Entities.Course>() on subject.SubjectId equals course.SubjectId
                join image in _repository.GetAll<Entities.CourseImage>() on course.CourseId equals image.CourseId
                where category.CategorytName == categoryName
                select new { course, image, subject }
            ).ToListAsync();  // 將查詢結果先轉換為 List

            // 在客戶端進行分組和選擇操作
            var groupedCourses = courses
                .GroupBy(c => c.subject.SubjectName)
                .Select(group => group.FirstOrDefault()) // 取每個 group 的第一個課程
                .Take(6)
                .ToList();

            return groupedCourses.Select(s => new CourseTopicTabViewModel
            {
                SubjectName = s.subject.SubjectName,
                TutorHeadShotImage = s.image.ImageUrl,
                TwentyFiveMinUnitPrice = s.course.TwentyFiveMinUnitPrice
            }).ToList();
        }





        public async Task<List<CourseTopicTabViewModel>> GetCourseCategoriesWithCoursesAsync()
        {
            var courseCategoriesWithCourses = await (
                from category in _repository.GetAll<Entities.CourseCategory>()
                join subject in _repository.GetAll<Entities.CourseSubject>() on category.CourseCategoryId equals subject.CourseCategoryId
                join course in _repository.GetAll<Entities.Course>() on subject.SubjectId equals course.SubjectId
                join image in _repository.GetAll<Entities.CourseImage>() on course.CourseId equals image.CourseId
                group course by new { subject.SubjectName, image.ImageUrl } into grouped
                select new CourseTopicTabViewModel
                {
                    SubjectName = grouped.Key.SubjectName,
                    TutorHeadShotImage = grouped.Key.ImageUrl,
                    TwentyFiveMinUnitPrice = grouped.Average(c => c.TwentyFiveMinUnitPrice) // 計算平均價格
                }).ToListAsync();

            return courseCategoriesWithCourses;
        }




    }
}
