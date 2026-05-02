using System.ComponentModel;
using Web.Entities;
using Web.Repository;
using Web.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Web.Services
{
    public class BookingService
    {
        private readonly IRepository _repository;
        public List<string> couresImagesList { get; set; }
        public BookingService(IRepository repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// 課程明細
        /// </summary>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public async Task<BookingListViewModel> GetPublishCourseList(int MemberId)
        {
            // 將 courseImg 實體化為 List<CouresImagesViewModel>
            var courseImg = await (from img in _repository.GetAll<Web.Entities.CourseImage>()
                                   join course in _repository.GetAll<Web.Entities.Course>() on img.CourseId equals course.CourseId
                                   where course.TutorId == MemberId
                                   select new CouresImagesVM
                                   {
                                       CourseImageId = img.CourseId,
                                       CourseId = course.CourseId,
                                       ImageUrl = img.ImageUrl
                                   }).ToListAsync();  // 確保 courseImg 是 List<CouresImagesViewModel>

            var bookingValue = from course in _repository.GetAll<Web.Entities.Course>()
                               where course.TutorId == MemberId
                               join category in _repository.GetAll<Web.Entities.CourseCategory>() on course.CategoryId equals category.CourseCategoryId
                               join subject in _repository.GetAll<Web.Entities.CourseSubject>() on course.SubjectId equals subject.SubjectId
                               //join image in _repository.GetAll<CourseImage>() on course.CourseId equals image.CourseId
                               join member in _repository.GetAll<Web.Entities.Member>() on course.TutorId equals member.MemberId
                               //join booking in _repository.GetAll<Booking>() on course.CourseId equals booking.CourseId
                               where member.MemberId == MemberId
                               orderby course.Cdate descending, course.Udate descending
                               select new BookingViewModel
                               {
                                   UpdateDatetime = course.Udate != null ? course.Udate : course.Cdate,
                                   CourseTitle = course.Title,  //這不確定是哪個欄位
                                   Category = category.CategorytName,
                                   CourseSubject = subject.SubjectName,
                                   Thumbnail = course.ThumbnailUrl,
                                   VideoUrl = course.VideoUrl,
                                   //CourseImageId = image.CourseImageId.ToString(),

                                   CouresImagesList = courseImg, // 直接指派 List

                                   CourseId = course.CourseId,
                                   Title = course.Title,
                                   SubTitle = course.SubTitle,
                                   TutorIntro = member.TutorIntro,
                                   Description = course.Description,
                                   TrialPriceNTD = 0,
                                   TwentyFiveMinPriceNTD = course.TwentyFiveMinUnitPrice,
                                   FiftyMinPriceNTD = course.FiftyMinUnitPrice,
                                   //BookingId = booking.BookingId,
                                   //BookingDate = booking.BookingDate,
                                   CourseLength = "", //這不確定是哪個欄位
                                   MemberName = $"{member.FirstName}, {member.LastName}"
                               };

            return new BookingListViewModel()
            {
                BookingList = await bookingValue.ToListAsync(),
            };


        }
        /// <summary>
        /// 歷史課程
        /// </summary>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public async Task<BookingListViewModel> GetPublishCourseHistoryList(int MemberId)
        {
            var bookingValue = from course in _repository.GetAll<Web.Entities.Course>()
                               join category in _repository.GetAll<Web.Entities.CourseCategory>() on course.CategoryId equals category.CourseCategoryId
                               join subject in _repository.GetAll<Web.Entities.CourseSubject>() on course.SubjectId equals subject.SubjectId
                               //join image in _repository.GetAll<CourseImage>() on course.CourseId equals image.CourseId
                               join member in _repository.GetAll<Web.Entities.Member>() on course.TutorId equals member.MemberId
                               join booking in _repository.GetAll<Web.Entities.Booking>() on course.CourseId equals booking.CourseId
                               where member.MemberId == MemberId
                               && booking.BookingDate < DateTime.Now.AddDays(1)
                               select new BookingViewModel
                               {
                                   UpdateDatetime = DateTime.Now,
                                   CourseTitle = course.Title,  //這不確定是哪個欄位
                                   Category = category.CategorytName,
                                   CourseSubject = subject.SubjectName,
                                   Thumbnail = course.ThumbnailUrl,
                                   VideoUrl = course.VideoUrl,
                                   //CourseImageId = image.CourseImageId.ToString(),
                                   CourseId = course.CourseId,
                                   Title = course.Title,
                                   SubTitle = course.SubTitle,
                                   TutorIntro = member.TutorIntro,
                                   Description = course.Description,
                                   TrialPriceNTD = 0,
                                   TwentyFiveMinPriceNTD = course.TwentyFiveMinUnitPrice,
                                   FiftyMinPriceNTD = course.FiftyMinUnitPrice,
                                   BookingId = booking.BookingId,
                                   BookingDate = booking.BookingDate,
                                   CourseLength = "", //這不確定是哪個欄位
                                   MemberName = $"{member.FirstName}, {member.LastName}"
                               };

            return new BookingListViewModel()
            {
                BookingList = await bookingValue.ToListAsync(),
            };
        }

        /// <summary>
        /// 課程明細By單筆
        /// </summary>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public async Task<CourseDataViewModel> GetPublishCourse(int MemberId, int CourseId)
        {
            // 將 courseImg 實體化為 List<CouresImagesViewModel>
            var courseImg =await (from img in _repository.GetAll<Web.Entities.CourseImage>()
                             join course in _repository.GetAll<Web.Entities.Course>() on img.CourseId equals course.CourseId
                             where course.TutorId == MemberId && course.CourseId == CourseId
                             select new CouresImagesVM
                             {
                                 ImageUrl = img.ImageUrl
                             }).ToListAsync();  // 確保 courseImg 是 List<CouresImagesViewModel>

            couresImagesList = courseImg.Select(x => x.ImageUrl).ToList();

            var bookingValue = from course in _repository.GetAll<Web.Entities.Course>()
                               join category in _repository.GetAll<Web.Entities.CourseCategory>() on course.CategoryId equals category.CourseCategoryId
                               join subject in _repository.GetAll<Web.Entities.CourseSubject>() on course.SubjectId equals subject.SubjectId
                               //join image in _repository.GetAll<CourseImage>() on course.CourseId equals image.CourseId
                               join member in _repository.GetAll<Web.Entities.Member>() on course.TutorId equals member.MemberId
                               //join booking in _repository.GetAll<Booking>() on course.CourseId equals booking.CourseId
                               where member.MemberId == MemberId && course.CourseId == CourseId
                               select new CourseDataViewModel
                               {
                                   //UpdateDatetime = DateTime.Now,
                                   //CourseTitle = course.Title,  //這不確定是哪個欄位
                                   //Category = category.CategorytName,
                                   CategoryId = course.CategoryId.ToString(),
                                   CategoryName = category.CategorytName,
                                   //CourseSubject = subject.SubjectName,
                                   SubjectName = subject.SubjectName,
                                   Thumbnail = course.ThumbnailUrl,
                                   VideoUrl = course.VideoUrl,
                                   //CourseImageId = image.CourseImageId.ToString(),
                                   //ThumbnailUrl = (List<string>)courseImg,
                                   //courseImageList = courseImg, // 直接指派 List

                                   CourseId = course.CourseId,
                                   Title = course.Title,
                                   SubTitle = course.SubTitle,
                                   //TutorIntro = member.TutorIntro,
                                   Description = course.Description,
                                   //TrialPriceNTD = 0,
                                   TwentyFiveMinPriceNTD = course.TwentyFiveMinUnitPrice.ToString("###"),
                                   FiftyMinPriceNTD = course.FiftyMinUnitPrice.ToString("###"),
                                   //BookingId = booking.BookingId,
                                   //BookingDate = booking.BookingDate,
                                   //CourseLength = "", //這不確定是哪個欄位
                                   //MemberName = $"{member.FirstName}, {member.LastName}",
                                   //MemberId = member.MemberId,
                                   SubjectId = subject.SubjectId,
                                   //CategoryId = course.CategoryId,
                                   CoursesStatus = course.CoursesStatus,
                                   CouresImagesList = couresImagesList
                               };

            return new CourseDataViewModel()
            {
                BookingList = await bookingValue.ToListAsync(),
            };

        }

        public async Task<int> GetRemainCourseQtyAsync(int memberId, int courseId)
        {
            //已購買課程數
            int purchasedQty = (
                from order in _repository.GetAll<Web.Entities.Order>()
                join orderDetail in _repository.GetAll<Web.Entities.OrderDetail>()
                on order.OrderId equals orderDetail.OrderId
                where order.MemberId == memberId && orderDetail.CourseId == courseId
                select (int)orderDetail.Quantity
                ).Sum();

            //已預約(使用)課程數
            int bookedQty = _repository.GetAll<Web.Entities.Booking>()
                .Where(bk => bk.CourseId == courseId && bk.StudentId == memberId)
                .Count();

            return purchasedQty - bookedQty;
        }

        public void CreateBookingData(int courseId, DateTime bookingDate, short bookingTime, int studentId)
        {
            var booking = new Web.Entities.Booking
            {
                CourseId = courseId,
                BookingDate = bookingDate,
                BookingTime = bookingTime,
                StudentId = studentId,
                Cdate = DateTime.Now,
                Udate = DateTime.Now,
                NotifyCount = 0
            };
            _repository.Create(booking);
            _repository.SaveChanges();
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            _repository.Update(booking);
            await _repository.SaveChangesAsync();
        }


        /// <summary>
        /// 課程新增或修改
        /// </summary>
        /// <param name="AddOrUpdate"></param>
        public async Task SaveCourse(CRUDStatus status, CourseDataViewModel courseData, int memberId)
        {
            try
            {
                if (status == CRUDStatus.Create)
                {
                    var course = new Course
                    {
                        CategoryId = int.Parse(courseData.CategoryId),
                        SubjectId = courseData.SubjectId,
                        TutorId = memberId,
                        Title = courseData.Title,
                        SubTitle = courseData.SubTitle,
                        TwentyFiveMinUnitPrice = decimal.Parse(courseData.TwentyFiveMinPriceNTD),
                        FiftyMinUnitPrice = decimal.Parse(courseData.FiftyMinPriceNTD),
                        Description = courseData.Description,
                        IsEnabled = courseData.IsEnabled,
                        ThumbnailUrl = courseData.ThumbnailUrl[0],
                        VideoUrl = courseData.VideoUrl,
                        CoursesStatus = courseData.CoursesStatus,
                        Cdate = DateTime.Now,
                    };
                    _repository.Create(course);
                    await _repository.SaveChangesAsync();

                    var courseId = course.CourseId;
                    foreach (var item in courseData.CouresImagesList)
                    {
                        var courseImg = new CourseImage
                        {
                            CourseId = courseId,
                            ImageUrl = item,
                            Cdate = DateTime.Now,
                        };
                        _repository.Create(courseImg);
                        await _repository.SaveChangesAsync();
                    }

                }
                else if (status == CRUDStatus.Update)
                {
                    CourseDataViewModel orgData = (await GetPublishCourse(memberId, courseData.CourseId)).BookingList[0];

                    var course = new Course
                    {
                        CourseId = courseData.CourseId,
                        CategoryId = int.Parse(orgData.CategoryId),
                        SubjectId = orgData.SubjectId,
                        TutorId = memberId,
                        Title = courseData.Title,
                        SubTitle = courseData.SubTitle,
                        TwentyFiveMinUnitPrice = decimal.Parse(courseData.TwentyFiveMinPriceNTD),
                        FiftyMinUnitPrice = decimal.Parse(courseData.FiftyMinPriceNTD),
                        Description = courseData.Description,
                        IsEnabled = orgData.IsEnabled,
                        ThumbnailUrl = orgData.Thumbnail,
                        VideoUrl = courseData.VideoUrl,
                        CoursesStatus = orgData.CoursesStatus,
                        Cdate = DateTime.Now,
                        Udate = DateTime.Now,
                    };
                    _repository.Update(course);
                    await _repository.SaveChangesAsync();

                    var courseId = course.CourseId;

                    ////先刪除圖檔
                    //var couresImg = from Img in _repository.GetAll<CourseImage>()
                    //                where Img.CourseId == courseId
                    //                select Img;
                    //_repository.Delete(couresImg);
                    //await _repository.SaveChangesAsync();

                    ////再存檔
                    //foreach (var item in courseData.CouresImagesList)
                    //{
                    //    var courseImg = new CourseImage
                    //    {
                    //        CourseId = courseId,
                    //        ImageUrl = item,
                    //        Cdate = DateTime.Now,
                    //    };
                    //    _repository.Create(courseImg);
                    //    await _repository.SaveChangesAsync();
                    //}
                }
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
            }
        }
        /// <summary>
        /// 取得課程科目
        /// </summary>
        /// <param name="courseCategoryId"></param>
        /// <returns></returns>
        public IEnumerable<CourseSubject> GetSubjectsByCategoryId(int courseCategoryId)
        {
            return _repository.GetAll<CourseSubject>()
                .Where(s => s.CourseCategoryId == courseCategoryId)
                .Select(s => new CourseSubject
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName
                }).ToList();
        }

        public async Task<List<Booking>> GetBookingsByDateAsync(DateTime date)
        {
            return await _repository.GetAll<Booking>()
                .Where(b => b.BookingDate == date)
                .ToListAsync();
        }
    }
}
