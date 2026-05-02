using Api.Dtos;
using Api.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CourseManagementApiDapperController : ControllerBase
    {
        private readonly CourseManagementApiDapperService _courseManagementApiService;

        public CourseManagementApiDapperController(CourseManagementApiDapperService courseManagementApiService)
        {
            _courseManagementApiService = courseManagementApiService;
        }

        [HttpGet]
        public async Task<ActionResult> GetCourseManagementData()
        {
            try
            {
                var courseManagementData = await _courseManagementApiService.GetCourseManagementData();
                return Ok(courseManagementData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCourseInfo([FromBody] UpdateCourseDto dto)
        {
            try
            {
                var result = await _courseManagementApiService.UpdateCourseInfo(dto);

                if (result)
                {
                    return Ok("更新課程資訊成功!");
                }
                else
                {
                    return BadRequest("更新課程資訊失敗!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        public async Task<ActionResult> UpdatePublishingStatus([FromBody] UpdatePublishingStatusDto dto)
        {
            try
            {
                var result = await _courseManagementApiService.UpdatePublishingStatus(dto.CourseId, dto.IsEnabled);

                if (result)
                {
                    return Ok("更新課程審核資訊成功!");
                }
                else
                {
                    return BadRequest("更新課程審核資訊失敗!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetCourseApprovalList()
        {
            try
            {
                var courseApprovalList = await _courseManagementApiService.GetCourseApprovalList();
                return Ok(courseApprovalList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCourseQty(bool startFromCurrentMonth)
        {
            try
            {
                var courseQty = await _courseManagementApiService.GetCourseQty(startFromCurrentMonth);
                return Ok(courseQty);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCourseQtyByPublishingStatus(bool isPublished, bool startFromCurrentMonth)
        {
            try
            {
                var courseQty = await _courseManagementApiService.GetCourseQtyByPublishingStatus(isPublished, startFromCurrentMonth);
                return Ok(courseQty);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCourseQtyByCoursesStatus(int coursesStatus, bool startFromCurrentMonth)
        {
            try
            {
                var courseQty = await _courseManagementApiService.GetCourseQtyByCoursesStatus(coursesStatus, startFromCurrentMonth);
                return Ok(courseQty);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        public async Task<ActionResult> UpdateCoursesStatus([FromBody] UpdateCoursesStatusDto dto)
        {
            try
            {
                var result = await _courseManagementApiService.UpdateCoursesStatus(dto.CourseId, dto.CourseApprove);

                if (result)
                {
                    return Ok("更新課程審核資訊成功!");
                }
                else
                {
                    return BadRequest("更新課程審核資訊失敗!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
