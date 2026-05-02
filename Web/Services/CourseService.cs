using ApplicationCore.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Web.Entities;
using Web.Dtos;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Web.Helpers;
using MongoDB.Driver.Linq;

namespace Web.Services
{
    public class CourseService
    {
        private readonly IRepository _repository;
        private readonly RedisCacheHelper _cacheHelper;

        public CourseService(IRepository repository, RedisCacheHelper cacheHelper)
        {
            _repository = repository;
            _cacheHelper = cacheHelper;
        }

        public async Task<CourseInfoListViewModel> GetCourseCardsListAsync(
            int page, 
            int pageSize, 
            int userId,
            string selectedSubject = null, 
            string selectedNation = null, 
            string selectedWeekdays = null, 
            string selectedTimeslots = null, 
            string selectedBudget = null,
            string selectedSortOption = null)
        {
            //課程主資訊查詢 & 套用篩選 (主資訊: 跟篩選相關的資訊)
            IQueryable<CourseInfoViewModel> courseMainInfoQuery = GetCourseMainInfoQuery();
            courseMainInfoQuery = await ApplyCourseMainInfoQueryFilters(courseMainInfoQuery, selectedSubject, selectedNation, selectedWeekdays, selectedTimeslots, selectedBudget, selectedSortOption);
            
            //取得篩選後的課程總數
            int totalCourseQty = await courseMainInfoQuery.CountAsync();

            //分頁
            //1. 取當前頁數的課程資訊
            List<CourseInfoViewModel> courseMainInfo = await courseMainInfoQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //2. 過濾當前頁面的CourseId及MemberId
            List<int> courseIds = courseMainInfo.Select(c => c.CourseId).ToList();
            List<int> memberIds = courseMainInfo.Select(c => c.MemberId).ToList();

            //3-1. 用已過濾後的Id做額外資訊查詢 (關注課程)
            var followingCoursesInfo = await GetFollowingStatusAsync(userId, courseIds);
            
            string followingStatusKey = string.Join("_", followingCoursesInfo.Where(f => f.FollowingStatus == true).Select(f => f.CourseId).ToList());
            
            //基於query string產生cache key
            string cacheKey = $"CourseList_{page}_{pageSize}_{userId}_{selectedSubject}_{selectedNation}_{selectedWeekdays}_{selectedTimeslots}_{selectedBudget}_{selectedSortOption}_{followingStatusKey}";
            //若cache有對應cache key的資料, 直接返回cached data
            var cachedData = await _cacheHelper.GetFromCacheAsync<CourseInfoListViewModel>(cacheKey);
            if (cachedData != null)
                return cachedData;

            //3-2. 用已過濾後的Id做額外資訊查詢 (課程圖片/教師已被預約時段/教師教課時段/關注課程)
            var courseImagesInfo = await GetCourseImagesAsync(courseIds);
            var bookedTimeSlotsInfo = await GetBookedTimeSlotsAsync(courseIds);
            var availableTimeSlotsInfo = await GetAvailableTimeSlotsAsync(memberIds);
            


            //4. 合併查詢 (只join分頁過的資訊)
            var completeCoursesInfo = (
                from courseMain in courseMainInfo
                join imgInfo in courseImagesInfo on courseMain.CourseId equals imgInfo.CourseId into imgInfoGroup
                from imgInfo in imgInfoGroup.DefaultIfEmpty()
                join bookInfo in bookedTimeSlotsInfo on courseMain.CourseId equals bookInfo.CourseId into bookInfoGroup
                from bookInfo in bookInfoGroup.DefaultIfEmpty()
                join tTimeInfo in availableTimeSlotsInfo on courseMain.MemberId equals tTimeInfo.MemberId into tTimeInfoGroup
                from tTimeInfo in tTimeInfoGroup.DefaultIfEmpty()
                join followingInfo in followingCoursesInfo on courseMain.CourseId equals followingInfo.CourseId into followingInfoGroup
                from followingInfo in followingInfoGroup.DefaultIfEmpty()
                select new CourseInfoViewModel
                {
                    CourseId = courseMain.CourseId,
                    TutorHeadShotImage = courseMain.TutorHeadShotImage,
                    TutorFlagImage = courseMain.TutorFlagImage,
                    NationName = courseMain.NationName,
                    IsVerifiedTutor = courseMain.IsVerifiedTutor,
                    CourseTitle = courseMain.CourseTitle,
                    CourseSubTitle = courseMain.CourseSubTitle,
                    TutorIntro = courseMain.TutorIntro,
                    TwentyFiveMinUnitPrice = courseMain.TwentyFiveMinUnitPrice,
                    FiftyMinUnitPrice = courseMain.FiftyMinUnitPrice,
                    CourseVideo = courseMain.CourseVideo,
                    CourseVideoThumbnail = courseMain.CourseVideoThumbnail,
                    SubjectName =courseMain.SubjectName,
                    CourseImages = imgInfo?.CourseImages ?? new List<CourseImageViewModel>(),
                    CourseRatings = courseMain.CourseRatings,
                    CourseReviews = courseMain.CourseReviews,
                    BookedTimeSlots = bookInfo?.BookedTimeSlots ?? new List<TimeSlotViewModel>(),
                    AvailableTimeSlots = tTimeInfo?.AvailableTimeSlots ?? new List<TimeSlotViewModel>(),
                    FollowingStatus = followingInfo.FollowingStatus
                }).ToList();

            var courseListQueryResult = new CourseInfoListViewModel
            {
                CourseInfoList = completeCoursesInfo,
                TotalCourseQty = totalCourseQty
            };

            //將查詢結果存進distributed cache (設定SlidingExpiration為5min, AbsoluteExpirationRelativeToNow為20分)
            await _cacheHelper.SetCacheAsync(cacheKey, courseListQueryResult, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(20));

            return courseListQueryResult;
        }

        //處理篩選
        private async Task<IQueryable<CourseInfoViewModel>> ApplyCourseMainInfoQueryFilters(
            IQueryable<CourseInfoViewModel> courseMainInfoQuery, 
            string selectedSubject, 
            string selectedNation, 
            string selectedWeekdays,
            string selectedTimeslots,
            string selectedBudget,
            string selectedSortOption)
        {
            //科目篩選
            if (!string.IsNullOrEmpty(selectedSubject))
            {
                courseMainInfoQuery = courseMainInfoQuery.Where(c => c.SubjectName == selectedSubject);
            }

            //國籍篩選
            if (!string.IsNullOrEmpty(selectedNation))
            {
                courseMainInfoQuery = courseMainInfoQuery.Where(c => c.NationName == selectedNation);
            }

            //時段篩選
            if (!string.IsNullOrEmpty(selectedWeekdays) || !string.IsNullOrEmpty(selectedTimeslots))
            {
                List<string> weekdayList = !string.IsNullOrEmpty(selectedWeekdays) ? selectedWeekdays.Split(',').ToList() : new List<string>();
                List<string> timeSlotList = !string.IsNullOrEmpty(selectedTimeslots) ? selectedTimeslots.Split(',').ToList() : new List<string>();
                List<int> memberIds = courseMainInfoQuery.Select(c => c.MemberId).ToList();
                var availableTimeSlotsInfo = await GetAvailableTimeSlotsAsync(memberIds);
                List<int> filteredMemberIds = new List<int>();

                //時間&星期共用篩選方法 (返回篩選後的MemberId集合)
                Func<string, string, IEnumerable<int>> FilterByTimeSlotAndWeekday = (string weekday, string timeslot) =>
                {
                    switch (timeslot)
                    {
                        case "6-12":
                            return availableTimeSlotsInfo
                                .Where(ts => ts.AvailableTimeSlots.Any(slot => (string.IsNullOrEmpty(weekday) || slot.Weekday == int.Parse(weekday)) && slot.StartHour >= 6 && slot.StartHour < 12))
                                .Select(ts => ts.MemberId);
                        case "12-18":
                            return availableTimeSlotsInfo
                                .Where(ts => ts.AvailableTimeSlots.Any(slot => (string.IsNullOrEmpty(weekday) || slot.Weekday == int.Parse(weekday)) && slot.StartHour >= 12 && slot.StartHour < 18))
                                .Select(ts => ts.MemberId);
                        case "18-24":
                            return availableTimeSlotsInfo
                                .Where(ts => ts.AvailableTimeSlots.Any(slot => (string.IsNullOrEmpty(weekday) || slot.Weekday == int.Parse(weekday)) && slot.StartHour >= 18 && slot.StartHour < 24))
                                .Select(ts => ts.MemberId);
                        case "0-6":
                            return availableTimeSlotsInfo
                                .Where(ts => ts.AvailableTimeSlots.Any(slot => (string.IsNullOrEmpty(weekday) || slot.Weekday == int.Parse(weekday)) && slot.StartHour >= 0 && slot.StartHour < 6))
                                .Select(ts => ts.MemberId);
                        default:
                            return Enumerable.Empty<int>();
                    }
                };
                
                //如果只選星期, 篩選星期
                if (string.IsNullOrEmpty(selectedTimeslots))
                {
                    foreach (string weekDay in weekdayList)
                    {
                        filteredMemberIds.AddRange(availableTimeSlotsInfo
                            .Where(ts => ts.AvailableTimeSlots.Any(slot => slot.Weekday == int.Parse(weekDay)))
                            .Select(ts => ts.MemberId).ToList());
                    }
                }

                //如果只選時間段, 篩選時間段
                else if (string.IsNullOrEmpty(selectedWeekdays))
                {
                    foreach (string timeSlot in timeSlotList)
                    {
                        filteredMemberIds.AddRange(FilterByTimeSlotAndWeekday(null, timeSlot));
                    }
                }

                //如果時間和日期同時選擇, 需判斷教師時間段符合對應星期&時間
                else
                {
                    foreach (string weekDay in weekdayList)
                    {
                        foreach (string timeSlot in timeSlotList)
                        {
                            filteredMemberIds.AddRange(FilterByTimeSlotAndWeekday(weekDay, timeSlot));
                        }
                    }                  
                }
                filteredMemberIds = filteredMemberIds.Distinct().ToList();
                courseMainInfoQuery = courseMainInfoQuery.Where(c => filteredMemberIds.Contains(c.MemberId));
            }

            //預算篩選
            if (!string.IsNullOrEmpty(selectedBudget))
            {
                switch (selectedBudget)
                {
                    case "349以下":
                        courseMainInfoQuery = courseMainInfoQuery.Where(c => c.TwentyFiveMinUnitPrice <= 349);
                        break;
                    case "350-499":
                        courseMainInfoQuery = courseMainInfoQuery.Where(c => c.TwentyFiveMinUnitPrice <= 499 && c.TwentyFiveMinUnitPrice >= 350);
                        break;
                    case "500-799":
                        courseMainInfoQuery = courseMainInfoQuery.Where(c => c.TwentyFiveMinUnitPrice <= 799 && c.TwentyFiveMinUnitPrice >= 500);
                        break;
                    case "800-999":
                        courseMainInfoQuery = courseMainInfoQuery.Where(c => c.TwentyFiveMinUnitPrice <= 999 && c.TwentyFiveMinUnitPrice >= 800);
                        break;
                    case "1000以上":
                        courseMainInfoQuery = courseMainInfoQuery.Where(c => c.TwentyFiveMinUnitPrice >= 1000);
                        break;
                    default:
                        break;
                }
            }

            //排序
            if (!string.IsNullOrEmpty(selectedSortOption))
            {
                switch (selectedSortOption)
                {
                    case "verifiedTutor":
                        courseMainInfoQuery = courseMainInfoQuery.OrderByDescending(c => c.IsVerifiedTutor);
                        break;
                    case "priceAscend":
                        courseMainInfoQuery = courseMainInfoQuery.OrderBy(c => c.TwentyFiveMinUnitPrice);
                        break;
                    case "reviewsCount":
                        courseMainInfoQuery = courseMainInfoQuery.OrderByDescending(c => c.CourseReviews);
                        break;
                    case "rating":
                        courseMainInfoQuery = courseMainInfoQuery.OrderByDescending(c => c.CourseRatings);
                        break;
                    default:
                        courseMainInfoQuery = courseMainInfoQuery.OrderBy(c => c.CourseId);
                        break;
                }             
            }
            return courseMainInfoQuery;
        }

        //課程主資訊查詢
        private IQueryable<CourseInfoViewModel> GetCourseMainInfoQuery()
        {
            return (
                from course in _repository.GetAll<Entities.Course>().AsNoTracking()
                where course.IsEnabled == true && course.CoursesStatus == 1
                join member in _repository.GetAll<Entities.Member>().AsNoTracking()
                on course.TutorId equals member.MemberId
                join subject in _repository.GetAll<Entities.CourseSubject>().AsNoTracking()
                on course.SubjectId equals subject.SubjectId
                join nation in _repository.GetAll<Entities.Nation>().AsNoTracking()
                on member.NationId equals nation.NationId
                join review in _repository.GetAll<Entities.Review>().AsNoTracking()
                on course.CourseId equals review.CourseId into gpReview

                select new CourseInfoViewModel
                {
                    MemberId = member.MemberId,
                    CourseId = course.CourseId,
                    TutorHeadShotImage = member.HeadShotImage,
                    NationName = nation.NationName,
                    TutorFlagImage = nation.FlagImage,
                    IsVerifiedTutor = member.IsVerifiedTutor,
                    CourseTitle = course.Title,
                    CourseSubTitle = course.SubTitle,
                    TutorIntro = member.TutorIntro,
                    TwentyFiveMinUnitPrice = course.TwentyFiveMinUnitPrice,
                    FiftyMinUnitPrice = course.FiftyMinUnitPrice,
                    CourseVideo = course.VideoUrl,
                    CourseVideoThumbnail = course.ThumbnailUrl,
                    SubjectName = subject.SubjectName,
                    CourseRatings = gpReview.Any()? Math.Round(gpReview.Average(cr => cr.Rating), 2) : 0,
                    CourseReviews = gpReview.Count(),
                });
        }


        // 課程圖片查詢 (by courseIds)
        private async Task<List<CourseInfoViewModel>> GetCourseImagesAsync(List<int> courseIds)
        {
            return await (
                from courseImage in _repository.GetAll<Entities.CourseImage>().AsNoTracking()
                where courseIds.Contains(courseImage.CourseId)
                group courseImage by courseImage.CourseId into gpImage
                select new CourseInfoViewModel
                {
                    CourseId = gpImage.Key,
                    CourseImages = gpImage.Select(ci => new CourseImageViewModel
                    {
                        ImageUrl = ci.ImageUrl
                    }).ToList()
                }).ToListAsync();
        }

        //已被預約時間查詢 (by courseIds)
        private async Task<List<CourseInfoViewModel>> GetBookedTimeSlotsAsync(List<int> courseIds)
        {
            return await (
                from booking in _repository.GetAll<Entities.Booking>().AsNoTracking()
                where courseIds.Contains(booking.CourseId)
                group booking by booking.CourseId into gpBooking
                select new CourseInfoViewModel
                {
                    CourseId = gpBooking.Key,
                    BookedTimeSlots = gpBooking.Select(cb => new TimeSlotViewModel
                    {
                        Date = cb.BookingDate,
                        StartHour = cb.BookingTime - 1 // id - 1 對應實際起始時間
                    }).ToList()
                }).ToListAsync();
        }

        // 教師可預約時間查詢 (by memberIds)
        private async Task<List<CourseInfoViewModel>> GetAvailableTimeSlotsAsync(List<int> memberIds)
        {
            return await(
                from tutorTimeSlot in _repository.GetAll<Entities.TutorTimeSlot>().AsNoTracking()
                where memberIds.Contains(tutorTimeSlot.TutorId)
                group tutorTimeSlot by tutorTimeSlot.TutorId into gpMember
                select new CourseInfoViewModel
                {
                    MemberId = gpMember.Key,
                    AvailableTimeSlots = gpMember.Select(mt => new TimeSlotViewModel
                    {
                        Weekday = mt.Weekday,
                        StartHour = mt.CourseHourId - 1,
                    }).ToList()
                }).ToListAsync();
        }

        //關注課程查詢 (by courseIds)
        public async Task<List<CourseInfoViewModel>> GetFollowingStatusAsync(int userId, List<int> courseIds)
        {      
            var watchedCourses = await _repository
                .GetAll<Entities.WatchList>().AsNoTracking()
                .Where(w => w.FollowerId == userId && courseIds.Contains(w.CourseId ?? -1))
                .Select(w => w.CourseId).ToListAsync();
            return courseIds.Select(courseId => new CourseInfoViewModel
            {
                CourseId = courseId,
                FollowingStatus = watchedCourses.Contains(courseId)
            }).ToList();
        }

        public async Task<CourseInfoViewModel> GetBookingTableAsync(int courseId)
        {
            var courseInfo = await _repository
                .GetAll<Entities.Course>()
                .AsNoTracking()
                .Where(course => course.CourseId == courseId)
                .Select(course => new CourseInfoViewModel
                {
                    CourseId = course.CourseId,
                    MemberId = course.TutorId,
                    TutorHeadShotImage = _repository
                        .GetAll<Entities.Member>()
                        .AsNoTracking()
                        .Where(m => m.MemberId == course.TutorId)
                        .Select(m => m.HeadShotImage)
                        .FirstOrDefault(),
                    CourseTitle = course.Title,
                    AvailableTimeSlots = _repository
                        .GetAll<Entities.TutorTimeSlot>()
                        .AsNoTracking()
                        .Where(ts => ts.TutorId == course.TutorId)
                        .Select(ts => new TimeSlotViewModel
                        {
                            Weekday = ts.Weekday,
                            StartHour = ts.CourseHourId
                        }).ToList(),
                    BookedTimeSlots = _repository
                        .GetAll<Entities.Booking>()
                        .AsNoTracking()
                        .Where(bk => bk.CourseId == course.CourseId)
                        .Select(bk => new TimeSlotViewModel
                        {
                            Date = bk.BookingDate,
                            StartHour = bk.BookingTime
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return courseInfo;
        }


        public decimal GetCourse25MinUnitPrice(int courseId)
        {
            return _repository.GetAll<Entities.Course>().AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .Select(c => c.TwentyFiveMinUnitPrice)
                .FirstOrDefault();
        }
        public bool IsWatched(int memberId, int courseId)
        {
            var IsFollowed = _repository.GetAll<Entities.WatchList>().Any(w => w.CourseId == courseId && w.FollowerId == memberId);
            return IsFollowed;

        }

        public async Task<CourseMainPageViewModel> GetCourseMainPage(int courseId, int memberId)
        {
            // 查詢課程、會員和國籍資料
            var courseMainInfo = await (
                from course in _repository.GetAll<Entities.Course>().AsNoTracking()
                join member in _repository.GetAll<Entities.Member>().AsNoTracking()
                on course.TutorId equals member.MemberId
                join nation in _repository.GetAll<Entities.Nation>().AsNoTracking()
                on member.NationId equals nation.NationId
                where course.CourseId == courseId
                select new CourseMainPageViewModel
                {
                    TutorId = member.MemberId,
                    CourseId = course.CourseId,
                    CategoryId = course.CategoryId,
                    EducationId= (int)member.EducationId,
                    SpokenLanguage = member.SpokenLanguage,
                    TutorHeadShotImage = member.HeadShotImage,
                    TutorFlagImage = nation.FlagImage,
                    IsVerifiedTutor = member.IsVerifiedTutor,
                    CourseTitle = course.Title,
                    CourseSubTitle = course.SubTitle, 
                    TutorIntro = member.TutorIntro,
                    CourseIntro = course.Description,
                    TwentyFiveMinPrice = (int)course.TwentyFiveMinUnitPrice,
                    FiftyMinPrice = (int)course.FiftyMinUnitPrice,
                    CourseVideo = course.VideoUrl,
                    CourseVideoThumbnail = course.ThumbnailUrl
                })
                .FirstOrDefaultAsync();         
            if (courseMainInfo == null)
                return null; // 如果找不到對應的課程資料，返回 null

            //完成課堂數查詢           
            var finishedCourses = await _repository.GetAll<Entities.Booking>()
                                    .Where(f => f.CourseId == courseId)
                                    .Where(f=>f.BookingDate < DateTime.Now)
                                    .CountAsync();

           //最高學歷的查詢
            var education = await _repository.GetAll<Entities.Education>()
                                .Where(w => w.EducationId == courseMainInfo.EducationId)
                                .AsNoTracking().ToListAsync();
            //專業證照的查詢
            var professional = await _repository.GetAll<Entities.ProfessionalLicense>()
                                .Where(p=>p.MemberId == courseMainInfo.TutorId)
                                .AsNoTracking().ToListAsync();
            var professionallist = professional.Any() ?
                                    professional.Select(p =>                                    
                                        new TutorProfessionList
                                        {
                                            ProfessionName = p.ProfessionalLicenseName
                                        }).ToList()
                                    : new List<TutorProfessionList>
                                    {
                                        new TutorProfessionList
                                        {
                                            ProfessionName = "目前無專業證照"
                                        }
                                    }.ToList();

            // 查詢該課程的評論
            var reviews = await (
                from comment in _repository.GetAll<Entities.Review>()
                join member in _repository.GetAll<Entities.Member>().AsNoTracking()
                on comment.StudentId equals member.MemberId
                where comment.CourseId == courseId
                orderby comment.Cdate descending
                select new ReviewViewModel
                {
                    ReviewerName = member.FirstName + " " + member.LastName,
                    CommentRating = comment.Rating,
                    ReviewDate = comment.Cdate.ToString("yyyy/MM/dd"),
                    ReviewContent = comment.CommentText
                }).ToListAsync();
            // 查詢教師的工作經驗
            var tutorExperiences = await _repository.GetAll<Entities.WorkExperience>()
                                .Where(w => w.MemberId == courseMainInfo.TutorId)
                                .AsNoTracking()
                                .ToListAsync();

            var recomCard = GetTutorRecommendCard(courseMainInfo.CategoryId);

            // 計算課程評分
            var averageRating = reviews.Any() ? reviews.Average(r => r.CommentRating) : 0;

            // 準備 CourseMainPageViewModel
            var courseMainPageViewModel = new CourseMainPageViewModel
            {
                CourseId = courseMainInfo.CourseId,
                TutorId = courseMainInfo.TutorId,
                TutorHeadShotImage = courseMainInfo.TutorHeadShotImage,
                TutorFlagImage = courseMainInfo.TutorFlagImage,
                IsVerifiedTutor = courseMainInfo.IsVerifiedTutor,
                CourseTitle = courseMainInfo.CourseTitle,
                CourseSubTitle = courseMainInfo.CourseSubTitle,
                TutorIntro = courseMainInfo.TutorIntro,
                CourseIntro = courseMainInfo.CourseIntro,
                TwentyFiveMinPrice = courseMainInfo.TwentyFiveMinPrice,
                FiftyMinPrice = courseMainInfo.FiftyMinPrice,
                CourseVideo = courseMainInfo.CourseVideo,
                CourseVideoThumbnail = courseMainInfo.CourseVideoThumbnail,
                CourseRatings = averageRating,
                CourseReviews = reviews.Count,
                FinishedCoursesTotal = finishedCourses, // 假設值，需從其他表查詢
                ExperienceList = tutorExperiences.Select(e => new TutorExperience
                {
                    StartYear = e.WorkStartDate.Year.ToString(),
                    EndYear = e.WorkEndDate.Year.ToString(),
                    WorkTitle = e.WorkName
                }).ToList(),
                EducationDegree = education.Select(w => new TutorEducationList
                {
                    StudyStartYear = w.StudyStartYear,
                    StudyEndYear = w.StudyEndYear,
                    SchoolAndDepartment = w.SchoolName + " " + w.DepartmentName,
                }).ToList(),
                CourseImages = new List<CourseImageViewModel>(), // 根據需求查詢並填充
                ProfessionList = professional.Select(p => new TutorProfessionList
                {
                    ProfessionName = p.ProfessionalLicenseName
                }).ToList(),
                FollowingStatus = IsWatched(memberId,courseId) ,
                TutorReconmmendCard = recomCard
            };

            // 處理折扣價錢
            var courseCountDiscountList = new List<CourseCountDiscount>
            {
                new CourseCountDiscount { CourseCount = 1, Discount = 0 },
                new CourseCountDiscount { CourseCount = 5, Discount = 5 },
                new CourseCountDiscount { CourseCount = 10, Discount = 10 },
                new CourseCountDiscount { CourseCount = 20, Discount = 15 }
            };

            courseMainPageViewModel.TwentyFiveDiscountedPrice = GettCoursePriceList(courseCountDiscountList, 25, courseMainPageViewModel.TwentyFiveMinPrice);
            courseMainPageViewModel.FiftyDiscountedPrice = GettCoursePriceList(courseCountDiscountList, 50, courseMainPageViewModel.FiftyMinPrice);


            return courseMainPageViewModel;
        }

        /// <summary>
        /// 25/50分鐘課程、價錢、折扣的方法
        /// </summary>
        private List<BaseDiscountPice> GettCoursePriceList(List<CourseCountDiscount> courseCounts, int time, decimal price)
        {
            return courseCounts.Select(x => new BaseDiscountPice
            {
                CourseCount = x.CourseCount,
                CourseDurance = time,
                Discount = (int)x.Discount,
                DiscountPrice = x.Discount == 0 ? price.ToString("N0") : (price * (1 - (x.Discount / 100))).ToString("N0"),
            }).ToList();
        }

        /// <summary>
        /// 首頁隨機顯示課程
        /// </summary>
        /// <returns></returns>
        public async Task<CourseInfoListViewModel> GetCourseList(string categoryName)
        {
            var courseList = await (from course in _repository.GetAll<Entities.Course>()
                                    join image in _repository.GetAll<Entities.CourseImage>() on course.CourseId equals image.CourseId
                                    join subject in _repository.GetAll<Entities.CourseSubject>() on course.SubjectId equals subject.SubjectId
                                    join category in _repository.GetAll<Entities.CourseCategory>() on subject.CourseCategoryId equals category.CourseCategoryId
                                    
                                    select new CourseInfoViewModel
                                    {
                                        CourseId = course.CourseId,
                                        SubjectId = subject.SubjectId,
                                        SubjectName = subject.SubjectName,
                                        TwentyFiveMinUnitPrice = course.TwentyFiveMinUnitPrice,
                                        TutorHeadShotImage = image.ImageUrl
                                    }).ToListAsync();

            return new CourseInfoListViewModel
            {
                CourseInfoList = courseList
            };
        }


        public double GetCourseRating(int courseId)
        {
            var courseRatings = _repository.GetAll<Entities.Review>()
                .Where(review => review.CourseId == courseId)
                .Select(review => (double)review.Rating);
            return courseRatings.Any() ? courseRatings.Average() : 0;
        }

        public List<TutorRecomCardList> GetTutorRecommendCard(int categoryId)
        {            
            var recomCardList = (from course in _repository.GetAll<Entities.Course>().AsNoTracking()
                                join member in _repository.GetAll<Entities.Member>().AsNoTracking()
                                on course.TutorId equals member.MemberId
                                join nation in _repository.GetAll<Entities.Nation>().AsNoTracking()
                                on member.NationId equals nation.NationId
                                where course.CategoryId == categoryId
                                where course.IsEnabled == true
                                where course.CoursesStatus == 1
                                select new TutorRecomCardList
                                {
                                    CourseId = course.CourseId,
                                    TutorHeadShot = member.HeadShotImage,
                                    NationFlagImg = nation.FlagImage,
                                    CourseTitle = course.Title,
                                    CourseSubTitle = course.SubTitle,
                                    TwentyFiveMinPrice = course.TwentyFiveMinUnitPrice.ToString("N0"),
                                    FiftyminPrice = course.FiftyMinUnitPrice.ToString("N0"),
                                    Description = course.Description,
                                }).ToList();


            var recomCardReview =  (
               from course in _repository.GetAll<Entities.Course>().AsNoTracking()
               join review in _repository.GetAll<Entities.Review>().AsNoTracking()
               on course.CourseId equals review.CourseId
               group review by course.CourseId into Review
               select new TutorRecomCardList
               {
                   CourseId = Review.Key,
                   Rating = Review.Any() ? Math.Round(Review.Average(cr => cr.Rating), 2) : 0,               
               }).ToList();

            foreach (var card in recomCardList)
            {
                var review = recomCardReview.FirstOrDefault(r => r.CourseId == card.CourseId);

                card.Rating = review?.Rating ?? 0;
            }
            
            return recomCardList;
        }

        public async Task<CourseReviewListDto> GetReviewList(int courseId)
        {
            // 查詢該課程的評論
            var reviews = await(
                from comment in _repository.GetAll<Entities.Review>()
                join member in _repository.GetAll<Entities.Member>().AsNoTracking()
                on comment.StudentId equals member.MemberId
                where comment.CourseId == courseId
                orderby comment.Cdate descending
                select new CourseReview
                {
                    ReviewerName = member.FirstName + " " + member.LastName,
                    CommentRating = (int)comment.Rating,
                    ReviewDate = comment.Cdate.ToString("yyyy/MM/dd"),
                    ReviewContent = comment.CommentText
                }).ToListAsync();

            return (new CourseReviewListDto
            {
                 CourseReviewList = reviews
            });




        }

        public ReviewButtonDto GetReviewInfo(int memberId, int courseId) 
        {
            var hasCommented = _repository.GetAll<Entities.Review>().Any(r => r.StudentId == memberId && r.CourseId == courseId);
            var hasTakenClass = _repository.GetAll<Entities.Booking>()
                                .AsEnumerable() // 轉換為記憶體中的集合
                                .Any(b => b.StudentId == memberId &&
                                          b.CourseId == courseId &&
                                          DateTime.Now >= b.BookingDate.Add(ConvertToTimeSpan(b.BookingTime)));
            var reviewButtonDto = new ReviewButtonDto
            {
                HasCommented = hasCommented,
                HasTakenClass = hasTakenClass,
            };
            return (reviewButtonDto);
        }

        private TimeSpan ConvertToTimeSpan(int bookingTime)
        {
            int hours = bookingTime / 100;  // 取得小時部分
            int minutes = bookingTime % 100; // 取得分鐘部分
            return new TimeSpan(hours, minutes, 0); // 建立 TimeSpan 物件
        }

        public void CreateReviews(int studentId, int courseId, byte NewReviewRating, string NewReviewContent)
        {
            try
            {
                var reviewEntity = new Entities.Review()
                {
                    StudentId = studentId,
                    Rating = NewReviewRating,
                    CourseId = courseId,
                    CommentText = NewReviewContent,
                    Cdate = DateTime.Now
                };
                _repository.Create<Entities.Review>(reviewEntity);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Review could not be created", ex);
            }

        }

        public async Task<string> GetCourseNameAsync(int courseId)
        {
            var course = await _repository.GetCourseByIdAsync(courseId);
            return course?.Title;
        }



    }
}




