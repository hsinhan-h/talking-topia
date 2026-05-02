
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System.ComponentModel;
using Web.Dtos;
using Web.Entities;
using Web.Repository;
using Web.ViewModels;
using static Web.ViewModels.TutorResumeViewModel;

namespace Web.Services
{
    public class ResumeDataService
    {
        private readonly IRepository _repository;
        private readonly CloudinaryService _cloudinaryService;
        public ResumeDataService(IRepository repository, CloudinaryService cloudinaryService)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
        }

        //Api需要的

        public async Task<(bool Success, string Message)> UpdateAIimgfuction(UpdateAIImgDto request)
        {
            try
            {
                // 查找會員
                var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == request.MemberId)
                            .FirstOrDefaultAsync();

                if (member == null)
                {
                    return (false, "未找到會員");
                }

                member.HeadShotImage = request.Url;  

                // 更新資料庫紀錄
                _repository.Update(member);
                await _repository.SaveChangesAsync();

                return (true, "更新成功");
            }
            catch (Exception ex)
            {
                throw new Exception("更新 AI 圖片 URL 失敗", ex);
            }
        }

        
        public async Task<AIImagesResponseDto> GetAIimgesList(int memberId)
        {
            var applyList = await _repository.GetAll<Entities.ApplyList>()
                .Where(a => a.MemberId == memberId)
                .Select(a => new AIImage
                {
                    Url1 = a.AiimageUrl1,
                    Url2 = a.AiimageUrl2,
                    Url3 = a.AiimageUrl3
                })
                .ToListAsync();

            if (!applyList.Any())  
            {
                return new AIImagesResponseDto
                {
                    Success = false,
                    Message = "找不到圖片資料。",
                    Data = null
                };
            }

            return new AIImagesResponseDto
            {
                Success = true,
                Message = "圖片資料取得成功。",
                Data = applyList
            };
        }

        public async Task<(bool Success, string Message)> CheckAIStatusFunction(int memberId)
        {
            try
            {
                var applyList = await GetApplyListByMemberIdAsync(memberId);

                if (applyList != null)
                {
                    // 更新已有的資料
                    if (applyList.AiimgageStatus == null)
                    {
                        applyList.AiimgageStatus = true;
                    }

                    _repository.Update(applyList);
                }
                else
                {
                    // 如果沒有該會員的資料，創建新的記錄
                    var newApplyList = new Entities.ApplyList
                    {
                        MemberId = memberId,
                        ApplyStatus = false,
                        ApplyDateTime = DateTime.Now,
                        ApprovedDateTime = null,
                        UpdateStatusDateTime = null,
                        RejectReason = null,
                        AiimgageStatus = true,
                    };

                    _repository.Create(newApplyList);
                }

                // 保存變更並提交
                await _repository.SaveChangesAsync();
  

                return (true, "AI status updated successfully.");
            }
            catch (Exception ex)
            {
                // 捕捉例外狀況並回傳失敗訊息
                return (false, $"Failed to update AI status: {ex.Message}");
            }
        }



        public async Task<TutorResumeViewModel> ChangeResumeLicenses(int memberId, List<int> licenseIds, List<string> licenseNames, List<string> licenseUrls)
        {
            try
            {
                // 查找會員資料
                var member = await _repository.GetAll<Entities.Member>()
                                    .Include(m => m.ProfessionalLicenses)
                                    .FirstOrDefaultAsync(m => m.MemberId == memberId);

                if (member == null)
                {
                    throw new Exception("Member not found.");
                }

                // 檢查 licenseIds 和其他參數是否為單一值的情況
                if (licenseIds.Count == 1 && licenseNames.Count == 1 && licenseUrls.Count == 1)
                {
                    // 單一證照處理
                    var licenseId = licenseIds[0];
                    var licenseName = licenseNames[0];
                    var licenseUrl = licenseUrls[0];

                    // 查找現有證照資料根據 ID
                    var existingLicense = member.ProfessionalLicenses.FirstOrDefault(pl => pl.ProfessionalLicenseId == licenseId);

                    if (existingLicense != null)
                    {
                        // 更新現有的證照資料
                        existingLicense.ProfessionalLicenseName = licenseName;
                        existingLicense.ProfessionalLicenseUrl = licenseUrl;
                        existingLicense.Udate = DateTime.Now; // 更新修改日期
                    }
                    else
                    {
                        // 如果找不到證照，則創建新的證照
                        var newLicense = new Entities.ProfessionalLicense
                        {
                            MemberId = memberId,
                            ProfessionalLicenseName = licenseName,
                            ProfessionalLicenseUrl = licenseUrl,
                            Cdate = DateTime.Now,
                            Udate = null
                        };
                        member.ProfessionalLicenses.Add(newLicense);
                    }
                }
                else
                {
                    // 遍歷前端提交的證照資料，處理多個證照的情況
                    for (int i = 0; i < licenseNames.Count; i++)
                    {
                        // 查找現有證照資料根據 ID
                        var existingLicense = member.ProfessionalLicenses
                            .FirstOrDefault(pl => pl.ProfessionalLicenseId == licenseIds[i]);

                        if (existingLicense != null)
                        {
                            // 更新現有的證照資料
                            existingLicense.ProfessionalLicenseName = licenseNames[i];
                            existingLicense.ProfessionalLicenseUrl = licenseUrls[i];
                            existingLicense.Udate = DateTime.Now; // 更新修改日期
                        }
                        else
                        {
                            // 如果找不到證照，則創建新的證照
                            var newLicense = new Entities.ProfessionalLicense
                            {
                                MemberId = memberId,
                                ProfessionalLicenseName = licenseNames[i],
                                ProfessionalLicenseUrl = licenseUrls[i],
                                Cdate = DateTime.Now,
                                Udate = null
                            };
                            member.ProfessionalLicenses.Add(newLicense);
                        }
                    }
                }

                // 保存變更
                await _repository.SaveChangesAsync();

                // 回傳成功的 ViewModel
                return new TutorResumeViewModel();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update licenses", ex);
            }
        }
        public async Task<int> CreateNewLicense(int memberId, string licenseName, List<string> licenseUrls)
        {

            // 檢查是否有可用的 licenseUrls，並使用第一個作為新證照的 URL
            var licenseUrl = licenseUrls.FirstOrDefault() ?? string.Empty;

            // 創建新的證照實體
            var newLicense = new Entities.ProfessionalLicense
            {
                MemberId = memberId,
                ProfessionalLicenseName = licenseName,
                ProfessionalLicenseUrl = licenseUrl,
                Cdate = DateTime.UtcNow,
                Udate = null
            };

            // 將新證照新增到資料庫
            _repository.Create(newLicense);

            // 保存變更
            await _repository.SaveChangesAsync();

            // 返回新生成的 ProfessionalLicenseId
            return newLicense.ProfessionalLicenseId;
        }

        public async Task<TutorResumeViewModel> DeleteLicensesAsync(int memberId, List<int> professionalLicenseIds)
        {
            // 查找會員資料
            var member = await _repository.GetAll<Entities.Member>()
                                .Include(m => m.ProfessionalLicenses)
                                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                return new TutorResumeViewModel
                {
                    Success = false,
                    Message = "Member not found."
                };
            }

            // 查找需要刪除的證照
            var licensesToDelete = member.ProfessionalLicenses
                .Where(pl => professionalLicenseIds.Contains(pl.ProfessionalLicenseId))
                .ToList();

            if (!licensesToDelete.Any())
            {
                return new TutorResumeViewModel
                {
                    Success = false,
                    Message = "No licenses found to delete."
                };
            }

            // 逐個刪除證照
            foreach (var license in licensesToDelete)
            {
                _repository.Delete(license);
            }

            // 保存變更
            await _repository.SaveChangesAsync();

            return new TutorResumeViewModel
            {
                Success = true,
                Message = "Licenses deleted successfully."
            };
        }
        public async Task<TutorResumeViewModel> DeleteWorkExpAsync(int memberId, List<int> workExperienceIds)
        {
            // 查找會員資料
            var member = await _repository.GetAll<Entities.Member>()
                                .Include(m => m.WorkExperiences)
                                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                return new TutorResumeViewModel
                {
                    Success = false,
                    Message = "Member not found."
                };
            }
            var workExpToDelete = member.WorkExperiences
                .Where(wexp => workExperienceIds.Contains(wexp.WorkExperienceId))
                .ToList();

            if (workExpToDelete.Count == 0)
            {
                return new TutorResumeViewModel
                {
                    Success = false,
                    Message = "No licenses found to delete."
                };
            }
            foreach (var wexp in workExpToDelete)
            {
                _repository.Delete(wexp);
            }
            await _repository.SaveChangesAsync();

            return new TutorResumeViewModel
            {
                Success = true,
                Message = "Licenses deleted successfully."
            };
        }

        public async Task<int> ChangeResumeWorkExp(int memberId, List<ResumeWorkExp> workExperiences, List<string> fileUrls)
        {
            try
            {
                // 查找會員資料
                var member = await _repository.GetAll<Entities.Member>()
                                     .Include(m => m.WorkExperiences)
                                     .FirstOrDefaultAsync(m => m.MemberId == memberId);

                if (member == null)
                {
                    throw new Exception("Member not found.");
                }

                // 處理單個工作經驗
                var workExp = workExperiences.First();  // 只會有一個工作經驗

                // 檢查是否有 WorkExperienceId，來決定是更新還是新增
                if (workExp.WorkExperienceId.HasValue && workExp.WorkExperienceId != 0)
                {
                    // 如果有 WorkExperienceId，則嘗試更新現有的資料
                    var existingWorkExp = member.WorkExperiences
                        .FirstOrDefault(wexp => wexp.WorkExperienceId == workExp.WorkExperienceId.Value);

                    if (existingWorkExp != null)
                    {
                        // 更新現有的資料
                        existingWorkExp.WorkName = workExp.WorkName;
                        existingWorkExp.WorkStartDate = (DateOnly)workExp.WorkStartDate;
                        existingWorkExp.WorkEndDate = (DateOnly)workExp.WorkEndDate;
                        existingWorkExp.WorkExperienceFile = fileUrls.FirstOrDefault();
                        existingWorkExp.Udate = DateTime.Now;
                    }
                    else
                    {
                        throw new Exception($"Work experience with ID {workExp.WorkExperienceId.Value} not found.");
                    }
                }
                else
                {
                    // 如果沒有 WorkExperienceId，則新增新的工作經驗資料
                    var newWorkExp = new Entities.WorkExperience
                    {
                        MemberId = memberId,
                        WorkName = workExp.WorkName,
                        WorkStartDate = (DateOnly)workExp.WorkStartDate,
                        WorkEndDate = (DateOnly)workExp.WorkEndDate,
                        WorkExperienceFile = fileUrls.FirstOrDefault(),
                        Cdate = DateTime.Now,
                        Udate = null
                    };
                    member.WorkExperiences.Add(newWorkExp);

                    // 保存變更，並返回新生成的 WorkExperienceId
                    await _repository.SaveChangesAsync();
                    return newWorkExp.WorkExperienceId;
                }

                // 保存變更
                await _repository.SaveChangesAsync();

                // 返回原本的 WorkExperienceId（更新情況下）
                return workExp.WorkExperienceId ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update work experiences", ex);
            }
        }

        public async Task ChangeHeadShotImage(int memberId, string headShotImageUrl)
        {
            var member = await _repository.GetAll<Entities.Member>()
                              .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member != null)
            {
                member.HeadShotImage = headShotImageUrl;
                _repository.Update(member);
                await _repository.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Member not found");
            }
        }
        public async Task<TutorResumeViewModel> GetProfessionalLicenseById(int memberId, int professionalLicenseId)
        {
            var query = from license in _repository.GetAll<Entities.ProfessionalLicense>()
                        where license.MemberId == memberId && license.ProfessionalLicenseId == professionalLicenseId
                        select new TutorResumeViewModel
                        {
                            ProlLicenseId = license.ProfessionalLicenseId,
                            ProlLicenseUrl = license.ProfessionalLicenseUrl
                        };

            var result = await query.FirstOrDefaultAsync();
            return result;
        }


        //Read需要的
        private async Task<TutorResumeViewModel> ReadHeadImg(int memberId)
        {
            var headimg = await _repository.GetAll<Entities.Member>()
                .Where(m => m.MemberId == memberId)
                .Select(m => new TutorResumeViewModel
                {
                    HeadShotImage = m.HeadShotImage
                })
                .FirstOrDefaultAsync();

            if (headimg == null)
            {
                var member = await _repository.GetAll<Entities.Member>()
                    .FirstOrDefaultAsync(m => m.MemberId == memberId);
                if (member != null)
                {
                    member.HeadShotImage = "";
                    _repository.Update(member);
                    await _repository.SaveChangesAsync();
                    return new TutorResumeViewModel
                    {
                        HeadShotImage = ""
                    };
                }
                else
                {
                    return new TutorResumeViewModel();
                }
            }

            return headimg;
        }

        private async Task<TutorResumeViewModel> ReadEducationAsync(int memberId)
        {
            var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == memberId)
                            .FirstOrDefaultAsync();

            if (member == null)
            {
                return new TutorResumeViewModel();
            }

            var education = await _repository.GetAll<Entities.Education>()
                    .Where(e => e.EducationId == member.EducationId)
                    .Select(e => new TutorResumeViewModel
                    {
                        SchoolName = e.SchoolName,
                        StudyEndYear = e.StudyEndYear,
                        StudyStartYear = e.StudyStartYear,
                        DepartmentName = e.DepartmentName
                    })
                    .FirstOrDefaultAsync();
            if (education == null)
            {
                return new TutorResumeViewModel();
            }

            var resumeEducaition = new TutorResumeViewModel
            {
                SchoolName = education.SchoolName,
                StudyEndYear = education.StudyEndYear,
                StudyStartYear = education.StudyStartYear,
                DepartmentName = education.DepartmentName,
            };
            return resumeEducaition;
        }

        public async Task<TutorResumeViewModel> ReadProfessionalLicense(int memberId)
        {
            // 查找會員
            var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == memberId)
                            .FirstOrDefaultAsync();

            if (member == null)
            {
                return new TutorResumeViewModel();
            }

            // 查找會員是否已有 ProfessionalLicense 資料
            var licenses = await _repository.GetAll<Entities.ProfessionalLicense>()
                                .Where(e => e.MemberId == member.MemberId)
                                .ToListAsync();

            // 如果會員尚無 ProfessionalLicense 資料，返回空值
            if (!licenses.Any())
            {
                return new TutorResumeViewModel
                {
                    ProfessionalLicenseName = new List<string>(),
                    ProfessionalLicenseUrl = new List<string>(),
                    ProfessionalLicenseId = new List<int>()
                };
            }

            // 提取 LicenseName, LicenseUrl, LicenseId
            var licenseNames = licenses.Select(e => e.ProfessionalLicenseName).ToList();
            var licenseUrls = licenses.Select(e => e.ProfessionalLicenseUrl).ToList();
            var licenseIds = licenses.Select(e => e.ProfessionalLicenseId).ToList();

            // 返回 ViewModel
            var resumeEducation = new TutorResumeViewModel
            {
                ProfessionalLicenseName = licenseNames,
                ProfessionalLicenseUrl = licenseUrls,
                ProfessionalLicenseId = licenseIds
            };

            return resumeEducation;
        }

        public async Task<TutorResumeViewModel> ReadWorkexp(int memberId)
        {
            // 查找會員
            var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == memberId)
                            .FirstOrDefaultAsync();

            if (member == null)
            {
                return new TutorResumeViewModel();
            }

            // 查找會員的工作經驗
            var workExpList = await _repository.GetAll<Entities.WorkExperience>()
                .Where(wexp => wexp.MemberId == member.MemberId)
                .Select(wexp => new ResumeWorkExp
                {
                    WorkName = wexp.WorkName,
                    WorkStartDate = wexp.WorkStartDate,
                    WorkEndDate = wexp.WorkEndDate,
                    WorkExperienceId = wexp.WorkExperienceId,
                })
                .ToListAsync();

            // 不再插入空白資料，只返回現有工作經驗資料
            var resumeViewModel = new TutorResumeViewModel
            {
                WorkBackground = workExpList
            };

            return resumeViewModel;
        }


        public async Task<TutorResumeViewModel> ReadApplyCourseData(int memberId)
        {
            var member = await _repository.GetAll<Entities.Member>()
                            .Where(m => m.MemberId == memberId)
                            .FirstOrDefaultAsync();

            if (member == null)
            {
                return new TutorResumeViewModel();
            }

            var applyCourseList = await _repository.GetAll<Entities.ApplyCourse>()
            .Where(ac => ac.MemberId == member.MemberId)
            .Select(ac => new ApplyCourseList
            {
                ApplyCourseCategoryId = ac.ApplyCourseCategoryId,
                ApplySubCategoryId = ac.ApplySubCategoryId,
            }).ToListAsync();

            var resumeViewModel = new TutorResumeViewModel
            {
                CourseList = applyCourseList.FirstOrDefault()
            };

            return resumeViewModel;
        }

        public async Task<TutorResumeViewModel> ReadAllTutorResumeAsync(int memberId)
        {
            var resumeViewModel = new TutorResumeViewModel();
            //取得頭貼網址
            var headImgFile = await ReadHeadImg(memberId);
            resumeViewModel.HeadShotImage = headImgFile.HeadShotImage;
            // 讀取教育資料
            var educationViewModel = await ReadEducationAsync(memberId);
            resumeViewModel.SchoolName = educationViewModel.SchoolName;
            resumeViewModel.StudyStartYear = educationViewModel.StudyStartYear;
            resumeViewModel.StudyEndYear = educationViewModel.StudyEndYear;
            resumeViewModel.DepartmentName = educationViewModel.DepartmentName;

            return resumeViewModel;
        }

        //Creat需要的
        private async Task<Entities.Education> GetEducationByIdAsync(int educationId)
        {
            var education = await _repository.GetAll<Entities.Education>()
                                             .FirstOrDefaultAsync(e => e.EducationId == educationId);

            if (education == null)
            {
                return null;
            }
            return education;
        }
        private async Task<Entities.ApplyCourse> GetApplyCouseByMemberIdAsync(int memberId)
        {
            var applyCourseHasMember = await _repository.GetAll<Entities.ApplyCourse>()
                                             .FirstOrDefaultAsync(w => w.MemberId == memberId);

            if (applyCourseHasMember == null)
            {
                return null;
            }
            return applyCourseHasMember;
        }
        private async Task<Entities.ApplyList> GetApplyListByMemberIdAsync(int memberId)
        {
            var applyListHasMember = await _repository.GetAll<Entities.ApplyList>()
                                             .FirstOrDefaultAsync(w => w.MemberId == memberId);

            if (applyListHasMember == null)
            {
                return null;
            }
            return applyListHasMember;
        }


        public async Task<TutorResumeViewModel> AddResumeAsync(TutorResumeViewModel qVM, int memberId)
        {
            await _repository.BeginTransActionAsync();
            try
            {

                // 檢查會員是否存在
                var existingMember = await _repository.GetMemberByIdAsync(memberId);
                if (existingMember == null)
                {
                    return new TutorResumeViewModel
                    {
                        Success = false,
                        Message = "找不到該會員，請檢查會員資料。",
                    };
                }

                // 更新會員資料
                existingMember.IsVerifiedTutor = false;
                existingMember.Cdate = DateTime.Now;
                existingMember.Udate = null;
                existingMember.FirstName = existingMember.FirstName ?? "N/A";
                existingMember.LastName = existingMember.LastName ?? "N/A";
                existingMember.Password = existingMember.Password ?? "N/A";
                existingMember.Email = existingMember.Email ?? "N/A";
                existingMember.Nickname = existingMember.Nickname ?? "N/A";
                existingMember.Phone = existingMember.Phone ?? "N/A";
                existingMember.Gender = (short)(existingMember.Gender != 0 ? existingMember.Gender : 0);
                existingMember.AccountType = (existingMember.AccountType != 0 ? existingMember.AccountType : 0);
                if (!existingMember.IsTutor)
                {
                    existingMember.IsTutor = false;
                }

                // 創建或更新教育經歷
                Entities.Education education;

                // 如果 Member 已有 EducationId，則更新教育經歷
                if (existingMember.EducationId.HasValue)
                {
                    // 根據 EducationId 查詢現有的教育經歷
                    education = await GetEducationByIdAsync(existingMember.EducationId.Value);
                    if (education == null)
                    {
                        return new TutorResumeViewModel
                        {
                            Success = false,
                            Message = "找不到該會員的教育經歷。",
                        };
                    }

                    // 更新現有教育經歷
                    education.SchoolName = qVM.SchoolName;
                    education.StudyStartYear = (int)qVM.StudyStartYear;
                    education.StudyEndYear = (int)qVM.StudyEndYear;
                    education.DepartmentName = qVM.DepartmentName;
                    education.Udate = DateTime.Now;
                    _repository.Update(education);
                }
                else
                {
                    // 創建新的教育經歷
                    education = new Entities.Education
                    {
                        SchoolName = qVM.SchoolName,
                        StudyStartYear = (int)qVM.StudyStartYear,
                        StudyEndYear = (int)qVM.StudyEndYear,
                        DepartmentName = qVM.DepartmentName,
                        Cdate = DateTime.Now,
                        Udate = DateTime.Now
                    };

                    _repository.Create(education);
                    await _repository.SaveChangesAsync();

                    existingMember.EducationId = education.EducationId;
                }

                var applyCourse = await GetApplyCouseByMemberIdAsync(existingMember.MemberId);
                if (applyCourse != null)
                {
                    // 如果找到 ApplyCourse 記錄，進行更新

                    applyCourse.ApplyCourseCategoryId = qVM.CourseList.ApplyCourseCategoryId;
                    applyCourse.ApplySubCategoryId = qVM.CourseList.ApplySubCategoryId;

                    // 更新 ApplyCourse 表中的其他欄位，根據需求
                    applyCourse.Udate = DateTime.Now; // 更新日期
                    _repository.Update(applyCourse);

                }

                // 創建課程類別
                else
                {
                    var newApplyCourse = new Entities.ApplyCourse
                    {
                        MemberId = existingMember.MemberId,  // 關聯到會員的 MemberId
                        ApplyCourseCategoryId = qVM.CourseList.ApplyCourseCategoryId,
                        ApplySubCategoryId = qVM.CourseList.ApplySubCategoryId,
                        Cdate = DateTime.Now,  // 創建日期
                        Udate = DateTime.Now   // 更新日期
                    };

                    _repository.Create(newApplyCourse);

                }

                //創建履歷申請
                var applyList = await GetApplyListByMemberIdAsync(existingMember.MemberId);

                if (applyList != null)
                {
                    if (applyList.ApplyStatus != true)
                    {
                        applyList.ApplyStatus = false;
                    }

                    if (applyList.ApplyDateTime == null)
                    {
                        applyList.ApplyDateTime = DateTime.Now;
                    }


                    if (applyList.UpdateStatusDateTime == null)
                    {
                        applyList.UpdateStatusDateTime = DateTime.Now;
                    }

                    if (string.IsNullOrEmpty(applyList.RejectReason))
                    {
                        applyList.RejectReason = null;
                    }

                    _repository.Update(applyList);
                }
                else
                {
                    var newApplyList = new Entities.ApplyList
                    {
                        MemberId = existingMember.MemberId,
                        ApplyStatus = false,
                        ApplyDateTime = DateTime.Now,
                        ApprovedDateTime = null,
                        UpdateStatusDateTime = null,
                        RejectReason = null,
                    };

                    _repository.Create(newApplyList);
                }


                await _repository.SaveChangesAsync();
                await _repository.CommitAsync();
                return new TutorResumeViewModel
                {
                    Success = true,
                    Message = "履歷已成功新增/更新。",
                };
            }
            catch (Exception ex)
            {
                // 發生錯誤時回滾並返回錯誤訊息
                await _repository.RollbackAsync();
                var errorMessage = $"資料處理發生錯誤: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | 內層錯誤: {ex.InnerException.Message}";
                }

                return new TutorResumeViewModel
                {
                    Success = false,
                    Message = errorMessage
                };
            }
        }
    }
}

