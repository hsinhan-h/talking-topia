using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MemberManagermentApiController : ControllerBase
    {
        private readonly MemberManagermentApiService _memberManagermentApiService;

        public MemberManagermentApiController(MemberManagermentApiService memberManagermentApiService)
        {
            _memberManagermentApiService = memberManagermentApiService;
        }
        [HttpGet]
        public async Task<ActionResult> GetMemberDataList()
        {
            try
            {
                var allmemberdatalList = await _memberManagermentApiService.GetMemberDataList();
                return Ok(allmemberdatalList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut]
        public async Task<ActionResult> UpdateMemberDataList([FromBody] MemberDataDto dto)
        {
            try
            {
                var result = await _memberManagermentApiService.UpdateMemberData(dto);

                if (result)
                {
                    return Ok("會員資料更新成功!");
                }
                else
                {
                    return BadRequest("會員資料更新失敗!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMemberAccoutType(int memberId)
        {
            try
            {
                var result = await _memberManagermentApiService.LockMemberAccess(memberId);

                if (result)
                {
                    return Ok("會員資料更新成功!");
                }
                else
                {
                    return BadRequest("會員資料更新失敗!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }








        [HttpGet]
        public async Task<ActionResult> GetTutorDataList()
        {
            try
            {
                var allTutordatalList = await _memberManagermentApiService.GetTutorDataList();
                return Ok(allTutordatalList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut]
        public async Task<IActionResult> ApproveTutorData([FromBody] UpdateTutorDataDto tutorDto)
        {
            if (tutorDto == null || tutorDto.MemberId <= 0)
            {
                return BadRequest("無效的數據。");
            }
            var result = await _memberManagermentApiService.ProcessTutorApplicationAsync(tutorDto);

            if (result.IsSuccess)
            {
                return Ok(result.Message);
            }

            return BadRequest(result.Message);
        }
        [HttpPut]
        public async Task<IActionResult> RejectTutorData([FromBody] UpdateTutorDataDto tutorDto)
        {
            if (tutorDto == null || tutorDto.MemberId <= 0)
            {
                return BadRequest("無效的數據。");
            }
            var result = await _memberManagermentApiService.ProcessTutorRejectAsync(tutorDto);

            if (result.IsSuccess)
            {
                return Ok(result.Message);
            }

            return BadRequest(result.Message);
        }
        [HttpGet]
        public async Task<ActionResult> TutorDataInformation()
        {
            try
            {
                var tutordatainformationList = await _memberManagermentApiService.GetTutorDataInformation();
                return Ok(tutordatainformationList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet]
        public async Task<ActionResult> MemberDataCountInformation()
        {
            try
            {
                var memberDataCount = await _memberManagermentApiService.GetMemberInformation();
                return Ok(memberDataCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetDbImgUrlInformaiton(int memberId)
        {
            try
            {
                var imgUrl = await _memberManagermentApiService.GetDbImgUrlinforamtionfun(memberId);
                return Ok(imgUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateImgbyAI([FromBody] UpdateImageDto dto)
        {
            if (dto == null || dto.MemberId <= 0 || dto.ImageUrls == null || !dto.ImageUrls.Any())
            {
                return BadRequest("無效的請求");
            }

            try
            {
                await _memberManagermentApiService.UpdateMemberImageUrlsAsync(dto.MemberId, dto.ImageUrls);
                return Ok("圖片 URL 已成功更新");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新失敗: {ex.Message}");
                return StatusCode(500, "更新圖片 URL 時發生錯誤");
            }
        }


    }
}
