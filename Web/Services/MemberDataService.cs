using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Web.Entities;
using Web.Repository;
using Web.ViewModels;
using Infrastructure.Data;
using Web.Data;
using Microsoft.AspNetCore.Identity;

namespace Web.Services
{
    public class MemberDataService
    {
        private readonly IRepository _repository;
        
        private readonly ILogger<MemberDataService> _logger;
        private readonly AccountService _accountService;

        public MemberDataService(IRepository repository,  ILogger<MemberDataService> logger, AccountService accountService)
        {
            _repository = repository;
            
            _logger = logger;

            _accountService = accountService;

        }

        public async Task<MemberProfileViewModel> GetMemberData(int memberId)
        {
            _logger.LogInformation($"開始查詢會員資料，會員ID: {memberId}");

            var member = await _repository.GetAll<Member>().FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                _logger.LogWarning($"找不到會員資料，會員ID: {memberId}");
                throw new Exception($"找不到會員資料，會員ID: {memberId}");
            }

            _logger.LogInformation($"成功查詢會員資料，會員ID: {memberId}");

            // 查詢會員的課程偏好
            var coursePrefer = await (from mp in _repository.GetAll<MemberPreference>()
                                      join cs in _repository.GetAll<CourseSubject>()
                                          on mp.SubjecId equals cs.SubjectId
                                      join cc in _repository.GetAll<CourseCategory>()
                                          on cs.CourseCategoryId equals cc.CourseCategoryId
                                      where mp.MemberId == member.MemberId
                                      select new CourseListViewModel
                                      {
                                          CategoryName = cc.CategorytName,
                                          SubjectName = cs.SubjectName
                                      }).ToListAsync();

            // 轉換為 MemberProfileViewModel，處理可能為 null 的欄位
            var memberProfile = new MemberProfileViewModel
            {
                ImageUrl = member.HeadShotImage ?? string.Empty, // 如果圖片為 null，使用空字串代替
                Nickname = member.Nickname ?? "未設定", // 處理暱稱為 null 的情況
                Birthday = member.Birthday.HasValue ? member.Birthday.Value : (DateTime?)null, // 若無生日資料，設為 null
                Gender = member.Gender == 1 ? "男" : "女",
                Account = member.Account ?? member.LineUserId,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                Phone = member.Phone,
                CoursePrefer = coursePrefer,
            };

            _logger.LogInformation($"成功查詢會員資料，MemberId: {memberId}");

            return memberProfile;
        }


        //public async Task UpdateMemberData(MemberProfileViewModel updatedData, int memberId)
        //{
        //    if (updatedData == null)
        //    {
        //        throw new ArgumentNullException(nameof(updatedData), "更新資料不能為空");
        //    }

        //    // 查找對應的會員
        //    var member = await _repository.GetAll<Member>()
        //                                  .Include(m => m.MemberPreferences)  // 加載會員的課程偏好
        //                                  .FirstOrDefaultAsync(m => m.MemberId == memberId);

        //    if (member == null)
        //    {
        //        throw new Exception("會員資料未找到");
        //    }

        //    // 檢查生日是否為 null 並且在有效範圍內
        //    if (updatedData.Birthday.HasValue && updatedData.Birthday.Value < new DateTime(1753, 1, 1))
        //    {
        //        throw new Exception("生日日期無效，應在 1753-01-01 之後");
        //    }

        //    // 更新基本資料
        //    member.Account = updatedData.Account ?? member.Account;
        //    member.FirstName = updatedData.FirstName ?? member.FirstName;
        //    member.LastName = updatedData.LastName ?? member.LastName;
        //    member.Nickname = updatedData.Nickname ?? member.Nickname;
        //    member.Email = updatedData.Email ?? member.Email;
        //    member.Phone = updatedData.Phone ?? member.Phone;
        //    member.Birthday = updatedData.Birthday;

        //    // 處理性別的更新
        //    if (!string.IsNullOrEmpty(updatedData.Gender))
        //    {
        //        member.Gender = short.Parse(updatedData.Gender);
        //    }



        //    // 先將所有的課程主題加載到內存中，避免每次查找都查詢數據庫
        //    var allCourseSubjects = await _repository.GetAll<CourseSubject>().ToListAsync();

        //    // 取得當前的課程偏好
        //    var existingPreferences = member.MemberPreferences.ToList();

        //    // 找出需要移除的偏好（即存在於當前的偏好，但不在更新資料中）
        //    var preferencesToRemove = existingPreferences
        //        .Where(p => !updatedData.CoursePrefer.Any(up => allCourseSubjects
        //            .Any(cs => cs.SubjectId == p.SubjecId && cs.SubjectName == up.SubjectName)))
        //        .ToList();

        //    // 逐項移除，而不是清空整個集合
        //    foreach (var preference in preferencesToRemove)
        //    {
        //        _repository.Delete(preference); // 使用 _repository 來逐項刪除
        //    }

        //    // 新增偏好
        //    foreach (var course in updatedData.CoursePrefer)
        //    {
        //        var existingPreference = existingPreferences
        //            .FirstOrDefault(p => allCourseSubjects
        //                .Any(cs => cs.SubjectId == p.SubjecId && cs.SubjectName == course.SubjectName));

        //        if (existingPreference == null)
        //        {
        //            var subject = allCourseSubjects.FirstOrDefault(cs => cs.SubjectName == course.SubjectName);

        //            if (subject != null)
        //            {
        //                member.MemberPreferences.Add(new MemberPreference
        //                {
        //                    MemberId = memberId,
        //                    SubjecId = subject.SubjectId,
        //                    Cdate = DateTime.Now,
        //                    Udate = DateTime.Now,
        //                });
        //            }
        //        }
        //    }

        //    try
        //    {
        //        // 儲存變更到資料庫
        //        _repository.Update(member);
        //        await _repository.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        throw new Exception("更新會員資料時發生錯誤", ex);
        //    }
        //}


        public async Task<(bool Success, string Message)> UpdateMemberData(MemberProfileViewModel updatedData, int memberId)
        {
            if (updatedData == null)
            {
                return (false, "更新資料不能為空");
            }

            // 查找對應的會員
            var member = await _repository.GetAll<Member>()
                                          .Include(m => m.MemberPreferences)  // 加載會員的課程偏好
                                          .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                return (false, "會員資料未找到");
            }

            // 檢查生日是否為 null 並且在有效範圍內
            if (updatedData.Birthday.HasValue && updatedData.Birthday.Value < new DateTime(1753, 1, 1))
            {
                return (false, "生日日期無效，應在 1753-01-01 之後");
            }

            // 更新基本資料
            member.Account = updatedData.Account ?? member.Account;
            member.FirstName = updatedData.FirstName ?? member.FirstName;
            member.LastName = updatedData.LastName ?? member.LastName;
            member.Nickname = updatedData.Nickname ?? member.Nickname;
            member.Email = updatedData.Email ?? member.Email;
            member.Phone = updatedData.Phone ?? member.Phone;
            member.Birthday = updatedData.Birthday;

            // 處理性別的更新
            if (!string.IsNullOrEmpty(updatedData.Gender))
            {
                member.Gender = short.Parse(updatedData.Gender);
            }

            // 先將所有的課程主題加載到內存中，避免每次查找都查詢數據庫
            var allCourseSubjects = await _repository.GetAll<CourseSubject>().ToListAsync();

            // 取得當前的課程偏好
            var existingPreferences = member.MemberPreferences.ToList();

            // 找出需要移除的偏好
            var preferencesToRemove = existingPreferences
                .Where(p => !updatedData.CoursePrefer.Any(up => allCourseSubjects
                    .Any(cs => cs.SubjectId == p.SubjecId && cs.SubjectName == up.SubjectName)))
                .ToList();

            foreach (var preference in preferencesToRemove)
            {
                _repository.Delete(preference);
            }

            // 新增偏好
            foreach (var course in updatedData.CoursePrefer)
            {
                var existingPreference = existingPreferences
                    .FirstOrDefault(p => allCourseSubjects
                        .Any(cs => cs.SubjectId == p.SubjecId && cs.SubjectName == course.SubjectName));

                if (existingPreference == null)
                {
                    var subject = allCourseSubjects.FirstOrDefault(cs => cs.SubjectName == course.SubjectName);

                    if (subject != null)
                    {
                        member.MemberPreferences.Add(new MemberPreference
                        {
                            MemberId = memberId,
                            SubjecId = subject.SubjectId,
                            Cdate = DateTime.Now,
                            Udate = DateTime.Now,
                        });
                    }
                }
            }

            try
            {
                _repository.Update(member);
                await _repository.SaveChangesAsync();
                return (true, "會員資料更新成功");
            }
            catch (DbUpdateException ex)
            {
                return (false, "更新會員資料時發生錯誤：" + ex.Message);
            }
        }


        public async Task<CourseMainPageViewModel> GetWatchList(int memberId)
        {
            var watchCardInfo = (from watch in _repository.GetAll<WatchList>().AsNoTracking()
                                 join course in _repository.GetAll<Course>().AsNoTracking()
                                 on watch.CourseId equals course.CourseId
                                 join member in _repository.GetAll<Member>().AsNoTracking()
                                 on course.TutorId equals member.MemberId
                                 join nation in _repository.GetAll<Nation>().AsNoTracking()
                                 on member.NationId equals nation.NationId
                                 join category in _repository.GetAll<CourseCategory>().AsNoTracking()
                                 on course.CategoryId equals category.CourseCategoryId
                                 where watch.FollowerId == memberId
                                 select new TutorRecomCardList
                                 {
                                     CourseId = course.CourseId,
                                     CategoryId = category.CourseCategoryId,
                                     TutorHeadShot = member.HeadShotImage,
                                     NationFlagImg = nation.FlagImage,
                                     CourseTitle = course.Title,
                                     CourseSubTitle = course.SubTitle,
                                     TwentyFiveMinPrice = course.TwentyFiveMinUnitPrice.ToString("N0"),
                                     FiftyminPrice = course.FiftyMinUnitPrice.ToString("N0"),
                                     Description = course.Description,
                                 }).ToList();

            var recomCardReview = (
               from course in _repository.GetAll<Course>().AsNoTracking()
               join review in _repository.GetAll<Review>().AsNoTracking()
               on course.CourseId equals review.CourseId
               group review by course.CourseId into Review
               select new TutorRecomCardList
               {
                   CourseId = Review.Key,
                   Rating = Review.Any() ? Math.Round(Review.Average(cr => cr.Rating), 2) : 0,
               }).ToList();

            foreach (var card in watchCardInfo)
            {
                var review = recomCardReview.FirstOrDefault(r => r.CourseId == card.CourseId);

                card.Rating = review?.Rating ?? 0;
            }
            var language = watchCardInfo.Where(w=>w.CategoryId == 1).OrderBy(w=>w.CourseId).ToList();
            var prgramming = watchCardInfo.Where(w=>w.CategoryId == 2).OrderBy(w => w.CourseId).ToList();
            var school = watchCardInfo.Where(w => w.CategoryId == 3).OrderBy(w => w.CourseId).ToList();
            var watchlist = new CourseMainPageViewModel
            {
                MemberId = memberId,
                LanguageWatchList = language,
                CodingWatchList = prgramming,
                SchoolWatchList = school,
            };
            return watchlist;
        }

        public bool IsWatched(int memberId, int courseId)
        {
            var IsFollowed = _repository.GetAll<WatchList>().Any(w => w.CourseId == courseId && w.FollowerId == memberId);
            return IsFollowed;
        }
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int memberId, string currentPassword, string newPassword)
        {
            // 使用 AccountService 取得會員資料
            var member = await _accountService.GetMemberByIdAsync(memberId);
            if (member == null)
            {
                return (false, "無法找到使用者");
            }

            var passwordHasher = new PasswordHasher<Web.Entities.Member>();

            // 驗證目前密碼
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(member, member.Password, currentPassword);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return (false, "目前密碼不正確");
            }

            // 設置新密碼，統一使用 PasswordHasher
            member.Password = passwordHasher.HashPassword(member, newPassword);

            // 更新資料庫
            _repository.Update(member);
            await _repository.SaveChangesAsync();

            return (true, "密碼修改成功");
        }

    }

    // 更新會員資料
}
public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}


//public async Task<MemberProfileViewModel> GetMemberData(string account)
//{
//    // 兩筆假資料

//    var memberData = new MemberProfileViewModel
//    {
//        ImageUrl = "https://example.com/image.jpg",
//        Nickname = "Sunny",
//        Birthday = new DateTime(1990, 5, 12),
//        Gender = "Female",
//        Account = "sunny123",
//        FirstName = "Anna",
//        LastName = "Wang",
//        Email = "anna.wang@example.com",
//        Phone = "0987654321",
//        CousePrefer = new List<CouseListViewModel>
//            {
//                new CouseListViewModel { CategorytName = "Language", SubjectName = "English" },
//                new CouseListViewModel { CategorytName = "Language", SubjectName = "Japanese" }
//            },
//    };

//    if (memberData == null)
//    {
//        throw new Exception("會員資料未找到");
//    }

//    // 將找到的會員資料填寫到 ViewModel 中
//    var result = new MemberProfileViewModel
//    {
//        ImageUrl = memberData.ImageUrl,
//        Nickname = memberData.Nickname,
//        Birthday = memberData.Birthday,
//        Gender = memberData.Gender,
//        Account = memberData.Account,
//        FirstName = memberData.FirstName,
//        LastName = memberData.LastName,
//        Email = memberData.Email,
//        Phone = memberData.Phone,
//        CousePrefer = memberData.CousePrefer,

//    };

//    return result;
//}




