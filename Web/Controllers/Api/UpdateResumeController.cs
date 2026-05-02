using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Dtos;
using Web.Services;
using static Web.ViewModels.TutorResumeViewModel;

namespace Web.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UpdateResumeController : ControllerBase
    {
        private readonly ResumeDataService _resumeDataService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IRepository _repository;

        public UpdateResumeController(ResumeDataService resumeDataService, CloudinaryService cloudinaryService, IRepository repository)
        {
            _resumeDataService = resumeDataService;
            _cloudinaryService = cloudinaryService;
            _repository = repository;
        }

        [HttpPost]

        public async Task<IActionResult> UpdateResumeLicenses([FromForm] ResumeDto model)
        {
            try
            {
                var files = Request.Form.Files;
                var fileUrls = new List<string>();

                // 遍歷每個文件，逐一上傳
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        // 使用 Cloudinary 上傳，並取得返回的 URL
                        var fileUrl = await _cloudinaryService.UploadImageAsync(file);
                        fileUrls.Add(fileUrl);
                    }
                }

                // 如果沒有上傳新文件，則使用原本的 URL 列表
                if (!fileUrls.Any())
                {
                    fileUrls = model.ProfessionalLicenseUrl;
                }

                // 檢查是否傳遞的是單個證照 ID，並將其轉換為列表
                var licenseIds = model.ProfessionalLicenseId?.Count > 0 ? model.ProfessionalLicenseId : new List<int> { 0 };
                var licenseNames = model.ProfessionalLicenseName?.Count > 0 ? model.ProfessionalLicenseName : new List<string> { string.Empty };
                var professionalLicenseUrls = fileUrls.Any() ? fileUrls : new List<string> { string.Empty };

                // 檢查 ProfessionalLicenseId 列表中是否有值
                var licenseId = licenseIds.FirstOrDefault();

                // 如果 ProfessionalLicenseId 為 0，表示這是新的證照
                if (licenseId == 0)
                {
                    // 在資料庫中插入新證照，並取得新生成的 ProfessionalLicenseId
                    var newLicenseId = await _resumeDataService.CreateNewLicense(model.MemberId, licenseNames.First(), professionalLicenseUrls);
                    return Ok(new { success = true, ProfessionalLicenseId = newLicenseId });
                }

                // 否則更新現有的證照條目
                await _resumeDataService.ChangeResumeLicenses(model.MemberId, licenseIds, licenseNames, professionalLicenseUrls);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteLicense([FromBody] ResumeDto request)
        {

            // 調用服務層來刪除證照
            var result = await _resumeDataService.DeleteLicensesAsync(request.MemberId, request.ProfessionalLicenseId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }


        [HttpPost]
        public async Task<IActionResult> CreateLicense([FromBody] ResumeDto request)
        {
            try
            {
                // 創建一個新的 ProfessionalLicense 並儲存到資料庫
                var newLicense = new ProfessionalLicense
                {
                    MemberId = request.MemberId,
                    ProfessionalLicenseName = string.Empty,
                    ProfessionalLicenseUrl = string.Empty,
                    Cdate = DateTime.Now,
                    Udate = null
                };

                // 將新證照保存到資料庫
                _repository.Create(newLicense);
                await _repository.SaveChangesAsync();

                // 返回新生成的 ProfessionalLicenseId
                return Ok(new { success = true, newLicense.ProfessionalLicenseId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateWorkExperience([FromBody] ResumeDto request)
        {

            try
            {
                // 創建一個新的 workexpericen 並儲存到資料庫
                var newWexp = new WorkExperience
                {
                    MemberId = request.MemberId,
                    WorkName = string.Empty,
                    WorkStartDate = new DateOnly(2024, 1, 1), // 初始化為空，稍後更新
                    WorkEndDate = new DateOnly(2024, 1, 1),  // 初始化為空，稍後更新
                    Cdate = DateTime.Now,
                    Udate = null
                };

                // 將新證照保存到資料庫
                _repository.Create(newWexp);
                await _repository.SaveChangesAsync();

                // 返回新生成的 ProfessionalLicenseId
                return Ok(new { success = true, newWexp.WorkExperienceId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteWorkExp([FromBody] ResumeDto request)
        {
            var workExperienceIds = request.WorkBackground
        .Where(wb => wb.WorkExperienceId.HasValue)  // 只選擇有 WorkExperienceId 的項目
        .Select(wb => wb.WorkExperienceId.Value)    // 取得 WorkExperienceId
        .ToList();

            if (!workExperienceIds.Any())
            {
                return BadRequest(new { success = false, message = "未找到任何可刪除的工作經歷。" });
            }

            // 呼叫 Service 方法進行刪除
            var result = await _resumeDataService.DeleteWorkExpAsync(request.MemberId, workExperienceIds);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResumeWorkExp([FromForm] ResumeDto model)
        {
            try
            {
                // 獲取上傳的文件（如果有多個文件）
                var files = Request.Form.Files;
                var fileUrls = new List<string>();

                // 遍歷每個文件，逐一上傳
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        // 使用 Cloudinary 上傳，並取得返回的 URL
                        var fileUrl = await _cloudinaryService.UploadFileAsync(file);
                        fileUrls.Add(fileUrl);
                    }
                }

                // 如果沒有上傳新文件，則使用原本的 URL 列表
                if (!fileUrls.Any())
                {
                    fileUrls = model.WorkBackground.Select(w => w.WorkExperienceFile).ToList();
                }

                // 確保 WorkBackground 有數據
                if (model.WorkBackground != null && model.WorkBackground.Any())
                {
                    // 遍歷 WorkBackground 列表，將 DTO 轉換為實體類 ResumeWorkExp
                    foreach (var workExperienceDto in model.WorkBackground)
                    {
                        // 將 DTO 轉換為實體
                        var workExperience = new ResumeWorkExp
                        {
                            WorkExperienceId = workExperienceDto.WorkExperienceId,
                            WorkName = workExperienceDto.WorkName,
                            WorkStartDate = workExperienceDto.WorkStartDate,
                            WorkEndDate = workExperienceDto.WorkEndDate,
                            WorkExperienceFile = workExperienceDto.WorkExperienceFile
                        };

                        // 調用服務層，傳遞轉換後的實體
                        var newWorkExperienceId = await _resumeDataService.ChangeResumeWorkExp(
                            model.MemberId,
                            new List<ResumeWorkExp> { workExperience },  // 傳遞單個工作經驗
                            fileUrls
                        );

                        // 返回新生成的 WorkExperienceId（如果是新增）
                        return Ok(new { success = true, workExperienceId = newWorkExperienceId });
                    }
                }
                else
                {
                    return BadRequest(new { success = false, message = "No work background data provided" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                // 記錄具體的異常信息，方便調試
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHeadShotImage([FromForm] ResumeDto model)
        {
            try
            {
                var files = Request.Form.Files;
                string fileUrl = null;  // 預設為 null

                // 如果有文件，則上傳圖片
                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    fileUrl = await _cloudinaryService.UploadImageAsync(file);
                }

                await _resumeDataService.ChangeHeadShotImage(model.MemberId, fileUrl);

                return Ok(new { success = true, fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetHeadShotImage(int memberId)
        {
            var member = await _repository.GetAll<Entities.Member>()
                .Where(m => m.MemberId == memberId)
                .Select(m => new { HeadShotImage = m.HeadShotImage })
                .FirstOrDefaultAsync();

            if (member != null && !string.IsNullOrEmpty(member.HeadShotImage))
            {
                return Ok(new { success = true, headShotImage = member.HeadShotImage });
            }
            else
            {
                return Ok(new { success = true, headShotImage = "" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DownloadImg([FromBody] ResumeDto model)
        {
            var professionalLicense = await _resumeDataService.GetProfessionalLicenseById(model.MemberId, model.ProlLicenseId);

            if (professionalLicense == null)
            {
                return NotFound(new { success = false, message = "Professional License not found" });
            }

            return Ok(new
            {
                success = true,
                professionalLicenseId = professionalLicense.ProlLicenseId,
                professionalLicenseUrl = professionalLicense.ProlLicenseUrl
            });
        }
        //AI相關
        [HttpPost]
        public async Task<IActionResult> CheckAIStatus([FromBody] MemberIdDto dto)
        {
            var result = await _resumeDataService.CheckAIStatusFunction(dto.MemberId);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetApplyListImages([FromQuery] MemberIdDto dto)
        {
            var result = await _resumeDataService.GetAIimgesList(dto.MemberId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data 
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAIimg([FromBody] UpdateAIImgDto dto)
        {
            var result = await _resumeDataService.UpdateAIimgfuction(dto);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}
