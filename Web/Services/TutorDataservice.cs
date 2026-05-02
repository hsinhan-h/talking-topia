using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Web.Entities;
using Web.Services;
using static Web.Services.TutorDataservice;

namespace Web.Services
{
    public class TutorDataservice
    {
        private readonly IRepository _repository;
        public TutorDataservice(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Isteacher(int? memberId)
        {
            if (memberId == null)
            {
                return false;
            }

            var istutor = await (from member in _repository.GetAll<Entities.Member>()
                                 where member.MemberId == memberId
                                 select member.IsTutor).FirstOrDefaultAsync();

            return istutor;

        }
        private async Task<TutorDataViewModel> GetTutorDataAsync(int? memberId)
        {

            if (await Isteacher(memberId))
            {
                var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == memberId)
                            .FirstOrDefaultAsync();

                var tutorTimeSlot = await _repository.GetAll<Entities.TutorTimeSlot>()
                    .Where(t => t.TutorId == memberId)
                    .FirstOrDefaultAsync();

                var nation = await _repository.GetAll<Entities.Nation>()
                    .Where(n => n.NationId == member.NationId)
                    .FirstOrDefaultAsync();

                var education = await _repository.GetAll<Entities.Education>()
                    .Where(edu => edu.EducationId == member.EducationId)
                    .Select(edu => new Educational
                    {
                        SchoolName = edu.SchoolName,
                        StudyStartYear = edu.StudyStartYear,
                        StudyEndYear = edu.StudyEndYear
                    }).ToListAsync();

                var workExperience = await _repository.GetAll<Entities.WorkExperience>() 
                    .Where(wexp => wexp.MemberId == memberId)
                    .Select(wexp => new WorkExp
                    {
                        WorkStartDate = wexp.WorkStartDate,
                        WorkEndDate = wexp.WorkEndDate,
                        WorkName = wexp.WorkName
                    }).ToListAsync();

                var license = await (from memb in _repository.GetAll<Entities.Member>()
                                     join proLi in _repository.GetAll<Entities.ProfessionalLicense>()
                                     on memb.MemberId equals proLi.MemberId into licenseGroup
                                     from proLi in licenseGroup.DefaultIfEmpty()
                                     where memb.MemberId == memberId && proLi != null
                                     select new LicenseData
                                     {
                                         ProfessionalLicenseName = proLi.ProfessionalLicenseName 
                                     }).ToListAsync();

                var tutorData = new TutorDataViewModel
                {
                    TutorId = tutorTimeSlot?.TutorId ?? 0,
                    NationID = nation?.NationId ?? 0,
                    NativeLanguage = member.NativeLanguage,
                    SpokenLanguage = member.SpokenLanguage,
                    BankAccount = member.BankAccount,
                    BankCode = member.BankCode,
                    EducationalBackground = education,
                    WorkBackground = workExperience,
                    License = license
                };

                return tutorData;
            }
            return null;
        }
        private async Task<TutorDataViewModel> GetTutorCourseDataAsync(int? memberId)
        {
            var tutorCourseData = new TutorDataViewModel
            {
                Course = new List<CategoryData>()
            };

            if (await Isteacher(memberId))
            {
                var member = await _repository.GetAll<Entities.Member>()
            .Where(m => m.MemberId == memberId)
            .FirstOrDefaultAsync();

                if (member == null)
                {
                    return null;
                }
                var applyCourses = await (from ac in _repository.GetAll<Entities.ApplyCourse>()
                                          join acc in _repository.GetAll<Entities.ApplyCourseCategory>()
                                          on ac.ApplyCourseCategoryId equals acc.ApplyCourseCategoryId
                                          join acsc in _repository.GetAll<Entities.ApplyCourseSubCategory>()
                                          on ac.ApplySubCategoryId equals acsc.ApplySubCategoryId
                                          where ac.MemberId == member.MemberId
                                          select new CategoryData
                                          {
                                              CategoryName = acc.ApplyCategoryName,
                                              SubjectName = acsc.ApplySubCategoryName
                                          }).ToListAsync();

                // 將查詢結果放入 ViewModel 的 Course 列表
                tutorCourseData.Course = applyCourses;
            }
            return tutorCourseData;
        }
        private async Task<TutorDataViewModel> GetCoursestatusAsync(int? memberId)
        {
            var tutorCourseData = new TutorDataViewModel();

            if (await Isteacher(memberId))
            {
                var member = await _repository.GetAll<Entities.Member>()
                    .Where(m => m.MemberId == memberId)
                    .FirstOrDefaultAsync();

                if (member == null)
                {
                    return new TutorDataViewModel();
                }

                var applyList = await _repository.GetAll<Entities.ApplyList>()
                    .Where(a => a.MemberId == memberId)
                    .FirstOrDefaultAsync();

                if (applyList != null)
                {
                    if (applyList.ApplyStatus)
                    {
                        tutorCourseData.Coursestatus = CourseStatus.已審核;
                    }
                    else if (!string.IsNullOrEmpty(applyList.RejectReason))  
                    {
                        tutorCourseData.Coursestatus = CourseStatus.申請駁回;
                    }
                    else
                    {
                        tutorCourseData.Coursestatus = CourseStatus.未審核;
                    }
                }
            }

            return tutorCourseData;
        }

        public async Task<TutorDataViewModel> GetAllInformationAsync(int? memberId)
        {

            var tutorData = await GetTutorDataAsync(memberId);
            if (tutorData == null)
            {
                return new TutorDataViewModel();
            }

            var tutorCourseData = await GetTutorCourseDataAsync(memberId);
            if (tutorCourseData == null)
            {
                return new TutorDataViewModel();
            }

            var tutorCourseStatus = await GetCoursestatusAsync(memberId);
            if (tutorCourseStatus == null)
            {
                return new TutorDataViewModel();
            }

            tutorData.Course = tutorCourseData.Course;
            tutorData.Coursestatus = tutorCourseStatus.Coursestatus;
            return tutorData;
        }

        public enum CourseStatus
        {
            未審核 = 0,
            已審核 = 1,
            申請駁回 = 2,
        }
        //取得可預約時段的方法
        public async Task<TutorDataViewModel> GetTutorReserveTimeAsync(int? memberId)
        {
            var tutortime = new TutorDataViewModel
            {
                AvailableReservation = new List<AvailReservation>()
            };
            if (!await Isteacher(memberId))
            {
                return tutortime;
            }

            // 如果是認證講師，查詢可用的預約時間
            tutortime.AvailableReservation = await (from member in _repository.GetAll<Entities.Member>()
                                                    join tutorTimeSloot in _repository.GetAll<Entities.TutorTimeSlot>()
                                                    on member.MemberId equals tutorTimeSloot.TutorId
                                                    join coursehour in _repository.GetAll<Entities.CourseHour>()
                                                    on tutorTimeSloot.CourseHourId equals coursehour.CourseHourId
                                                    where member.MemberId == memberId
                                                    select new AvailReservation
                                                    {
                                                        Weekday = tutorTimeSloot.Weekday,
                                                        Coursehours = coursehour.Hour,
                                                    }).ToListAsync();

            return tutortime;
        }


        public async Task<TutorDataViewModel> CreateTutorData(TutorDataViewModel qVM, int memberId)
        {


            // 開始一個資料庫交易
            await _repository.BeginTransActionAsync();
            try
            {
                var existingMember = await _repository.GetMemberByIdAsync(memberId);
                if (existingMember == null)
                {
                    return new TutorDataViewModel
                    {
                        Success = false,
                        Message = "找不到該會員，請檢查會員資料。",
                    };
                }

                existingMember.NativeLanguage = qVM.NativeLanguage ?? existingMember.NativeLanguage;
                existingMember.SpokenLanguage = qVM.SpokenLanguage ?? existingMember.SpokenLanguage;
                existingMember.BankAccount = qVM.BankAccount ?? existingMember.BankAccount;
                existingMember.BankCode = qVM.BankCode ?? existingMember.BankCode;
                existingMember.Udate = DateTime.Now;
                existingMember.NationId = qVM.NationID;
                existingMember.FirstName = existingMember.FirstName ?? "N/A";
                existingMember.LastName = existingMember.LastName ?? "N/A";
                existingMember.Password = existingMember.Password ?? "N/A";
                existingMember.Email = existingMember.Email ?? "N/A";
                existingMember.Nickname = existingMember.Nickname ?? "N/A";
                existingMember.Phone = existingMember.Phone ?? "N/A";
                existingMember.Gender = (short)(existingMember.Gender != 0 ? existingMember.Gender : 0);
                if (existingMember.AccountType != 0 && (existingMember.AccountType == 1 || existingMember.AccountType == 2))
                {
                    existingMember.AccountType = existingMember.AccountType;
                }
                else
                {
                    existingMember.AccountType = 0;
                }
                existingMember.IsTutor = (existingMember.IsTutor == true || existingMember.IsTutor);
                //existingMember.IsTutor = true;
                existingMember.IsVerifiedTutor = (existingMember.IsVerifiedTutor != false && existingMember.IsVerifiedTutor);

                // 使用 Repository 來新增資料
                _repository.Update(existingMember);
                await _repository.SaveChangesAsync();

                // 提交交易
                await _repository.CommitAsync();

                // 返回成功結果
                qVM.Success = true;
                qVM.Message = "會員資料新增成功";
            }
            catch (Exception ex)
            {
                // 發生錯誤，回滾交易
                await _repository.RollbackAsync();
                qVM.Success = false;
                qVM.Message = $"資料處理發生錯誤: {ex.Message}";
                if (ex.InnerException != null)
                {
                    qVM.Message += $" | 內層錯誤: {ex.InnerException.Message}";
                }
            }
            return qVM;
        }

        public async Task<Entities.TutorTimeSlot> GetTutorTimeSlotAsync(int tutorId, int courseHourId, int weekday)
        {
            return await _repository.GetAll<Entities.TutorTimeSlot>()
                            .FirstOrDefaultAsync(x => x.TutorId == tutorId
                                                    && x.CourseHourId == courseHourId
                                                    && x.Weekday == weekday);
        }
        public async Task<TutorDataViewModel> CreateTutorTimeData(TutorDataViewModel qVM, int memberId)
        {
            await _repository.BeginTransActionAsync();
            try
            {
                var existingMember = await _repository.GetMemberByIdAsync(memberId);
                if (existingMember == null)
                {
                    qVM.Success = false;
                    qVM.Message = "找不到該會員，請檢查會員資料。";
                    return qVM;
                }

                // 1. 從資料庫取得該會員的所有時段資料
                var existingSlots = await _repository.GetAll<Entities.TutorTimeSlot>()
                    .Where(x => x.TutorId == memberId)
                    .ToListAsync();

                // 2. 收集前端提交的所有時段
                var submittedSlots = new List<(int Weekday, int CourseHourId)>();
                if (qVM.Schedule != null && qVM.Schedule.Count > 0)
                {
                    foreach (var schedule in qVM.Schedule.Values)
                    {
                        foreach (var courseHourId in schedule.CouseHoursId)
                        {
                            submittedSlots.Add((schedule.Weekday, courseHourId));
                        }
                    }
                }

                // 3. 找出需要刪除的時段（資料庫有但前端沒有）
                var slotsToDelete = existingSlots
                    .Where(slot => !submittedSlots.Contains((slot.Weekday, slot.CourseHourId)))
                    .ToList();

                foreach (var slot in slotsToDelete)
                {
                    _repository.Delete(slot);
                }

                // 4. 找出需要新增的時段（前端有但資料庫沒有）
                foreach (var (weekday, courseHourId) in submittedSlots)
                {
                    var existingSlot = existingSlots
                        .FirstOrDefault(x => x.CourseHourId == courseHourId && x.Weekday == weekday);

                    if (existingSlot == null)
                    {
                        var newSlot = new Entities.TutorTimeSlot
                        {
                            TutorId = memberId,
                            CourseHourId = courseHourId,
                            Weekday = weekday,
                            Cdate = DateTime.Now
                        };
                        _repository.Create(newSlot);
                    }
                    else
                    {
                        // 更新現有的時段資料
                        existingSlot.Cdate = DateTime.Now;
                        _repository.Update(existingSlot);
                    }
                }

                // 5. 保存變更
                await _repository.SaveChangesAsync();
                await _repository.CommitAsync();

                qVM.Success = true;
                qVM.Message = "教師資料已成功更新";
            }
            catch (Exception ex)
            {
                await _repository.RollbackAsync();
                qVM.Success = false;
                qVM.Message = $"資料處理發生錯誤: {ex.Message}";
                if (ex.InnerException != null)
                {
                    qVM.Message += $" | 內層錯誤: {ex.InnerException.Message}";
                }
            }
            return qVM;
        }


        public async Task<TutorDataViewModel> DeleteTimeSlotsForMember(int memberId)
        {
            var tutortime = new TutorDataViewModel
            {
                AvailableReservation = new List<AvailReservation>()
            };
            var timeSlots = await (from tutorTimeSloot in _repository.GetAll<Entities.TutorTimeSlot>()
                                   where tutorTimeSloot.TutorId == memberId
                                   select tutorTimeSloot).ToListAsync();
            //tutortime.AvailableReservation = (from tutorTimeSloot in timeSlots
            //                                        join coursehour in _repository.GetAll<Entities.CourseHour>()
            //                                        on tutorTimeSloot.CourseHourId equals coursehour.CourseHourId
            //                                        select new AvailReservation
            //                                        {
            //                                            Weekday = tutorTimeSloot.Weekday,
            //                                            Coursehours = coursehour.Hour,
            //                                        }).ToList();

            foreach (var timeSlot in timeSlots)
            {
                _repository.Delete(timeSlot);
            }
            await _repository.SaveChangesAsync();

            return tutortime; 
        }
    }
    
}
