using Api.Dtos;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Dapper;
using Humanizer;
using System.Data;

namespace Api.Services
{
    public class CourseManagementApiDapperService
    {
        private readonly IDbConnection _dbConnection;

        public CourseManagementApiDapperService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }


        public async Task<List<CourseApprovalDto>> GetCourseApprovalList()
        {
            var unApprovedCourses = await _dbConnection.QueryAsync<Course>(
                "SELECT * FROM Courses WHERE CoursesStatus = 0");

            //提取篩選過後課程的Id
            var courseIds = unApprovedCourses.Select(c => c.CourseId).ToList();
            var categoryIds = unApprovedCourses.Select(c => c.CategoryId).ToList();
            var subjectIds = unApprovedCourses.Select(c => c.SubjectId).ToList();
            var memberIds = unApprovedCourses.Select(c => c.TutorId).ToList();

            //只抓unApprovedCourses的images, category, subject
            var images = await _dbConnection.QueryAsync<CourseImage>("SELECT * FROM CourseImages WHERE CourseId IN @courseIds", new { courseIds });
            var categories = await _dbConnection.QueryAsync<CourseCategory>("SELECT * FROM CourseCategories WHERE CourseCategoryId IN @categoryIds", new { categoryIds });
            var subjects = await _dbConnection.QueryAsync<CourseSubject>("SELECT * FROM CourseSubjects WHERE SubjectId IN @subjectIds", new { subjectIds });
            var tutors = await _dbConnection.QueryAsync<Member>("SELECT * FROM Members WHERE MemberId IN @memberIds", new { memberIds });

            var courseApprovalList =
                from c in unApprovedCourses
                join img in images on c.CourseId equals img.CourseId into courseImages
                from courseImage in courseImages.DefaultIfEmpty()
                join ct in categories on c.CategoryId equals ct.CourseCategoryId into courseCategories
                from courseCategory in courseCategories.DefaultIfEmpty()
                join s in subjects on c.SubjectId equals s.SubjectId into courseSubjects
                from courseSubject in courseSubjects.DefaultIfEmpty()
                join t in tutors on c.TutorId equals t.MemberId into courseTutors
                from courseTutor in courseTutors.DefaultIfEmpty()
                select new CourseApprovalDto
                {
                    CourseId = c.CourseId,
                    TutorName = courseTutor != null ? courseTutor.FirstName + " " + courseTutor.LastName : "教師名稱不存在",
                    ApplyDate = c.Cdate,
                    CourseCategory = courseCategory != null ? courseCategory.CategorytName : "其他",
                    CourseSubject = courseSubject.SubjectName != null ? courseSubject.SubjectName : "其他",
                    CourseTitle = c.Title,
                    TwentyFiveMinUnitPrice = c.TwentyFiveMinUnitPrice,
                    FiftyMinUnitPrice = c.FiftyMinUnitPrice,
                    Description = c.Description,
                    CourseImages = courseImages.Select(i => i.ImageUrl).ToList(),
                    ThumbnailUrl = c.ThumbnailUrl,
                    VideoUrl = c.VideoUrl
                };

            return courseApprovalList
                .GroupBy(c => c.CourseId)
                .Select(gp => gp.First())
                .ToList();
        }

        public async Task<int> GetCourseQtyByPublishingStatus(bool isPublished, bool startFromCurrentMonth)
        {
            var query = @"
                SELECT COUNT(*) FROM Courses
                WHERE IsEnabled = @isPublished AND CoursesStatus = 1";
            if (startFromCurrentMonth)
            {
                query += " AND Cdate >= @firstDayOfCurrentMonth";
            }
            var firstDayOfCurrentMonth = startFromCurrentMonth? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : (DateTime?)null;

            return await _dbConnection.ExecuteScalarAsync<int>(query, new
            {
                isPublished,
                firstDayOfCurrentMonth
            });
        }

        public async Task<int> GetCourseQty(bool startFromCurrentMonth)
        {
            var query = @"
                SELECT COUNT(*) FROM Courses
                WHERE CoursesStatus != 2";
            if (startFromCurrentMonth)
            {
                query += " AND Cdate >= @firstDayOfCurrentMonth";
            }
            var firstDayOfCurrentMonth = startFromCurrentMonth ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : (DateTime?)null;

            return await _dbConnection.ExecuteScalarAsync<int>(query, new
            {
                firstDayOfCurrentMonth
            });
        }


        public async Task<int> GetCourseQtyByCoursesStatus(int coursesStatus, bool startFromCurrentMonth)
        {
            var query = @"
                SELECT COUNT(*) FROM Courses
                WHERE CoursesStatus = @coursesStatus";

            if (startFromCurrentMonth)
            {
                query += " AND Cdate >= @firstDayOfCurrentMonth";
            }
            var firstDayOfCurrentMonth = startFromCurrentMonth ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : (DateTime?)null;

            return await _dbConnection.ExecuteScalarAsync<int>(query, new
            {
                coursesStatus,
                firstDayOfCurrentMonth
            });
        }


        public async Task<List<CourseManagementDto>> GetCourseManagementData()
        {
            var validCourses = await _dbConnection.QueryAsync<Course>(
                "SELECT * FROM Courses WHERE CoursesStatus != 2");
            var images = await _dbConnection.QueryAsync<CourseImage>("SELECT * FROM CourseImages");
            var categories = await _dbConnection.QueryAsync<CourseCategory>("SELECT * FROM CourseCategories");
            var subjects = await _dbConnection.QueryAsync<CourseSubject>("SELECT * FROM CourseSubjects");
            var tutors = await _dbConnection.QueryAsync<Member>("SELECT * FROM Members");

            var courseManagementData =
                from c in validCourses
                join img in images on c.CourseId equals img.CourseId into courseImages
                from courseImage in courseImages.DefaultIfEmpty()
                join ct in categories on c.CategoryId equals ct.CourseCategoryId into courseCategories
                from courseCategory in courseCategories.DefaultIfEmpty()
                join s in subjects on c.SubjectId equals s.SubjectId into courseSubjects
                from courseSubject in courseSubjects.DefaultIfEmpty()
                join t in tutors on c.TutorId equals t.MemberId into courseTutors
                from courseTutor in courseTutors.DefaultIfEmpty()
                select new CourseManagementDto
                {
                    CourseId = c.CourseId,
                    CourseTitle = c.Title,
                    CourseSubTitle = c.SubTitle,
                    TutorName = courseTutor != null ? courseTutor.FirstName + " " + courseTutor.LastName : "教師名稱不存在",
                    CourseCategory = courseCategory != null ? courseCategory.CategorytName : "其他",
                    CourseSubject = courseSubject.SubjectName != null ? courseSubject.SubjectName : "其他",
                    Description = c.Description,
                    CourseImages = courseImages.Select(i => i.ImageUrl).ToList(),
                    PublishStatus = c.IsEnabled && c.CoursesStatus == 1,
                    PublishDate = c.Cdate,
                    IsUnderReview = c.CoursesStatus == 0,
                    TwentyFiveMinUnitPrice = c.TwentyFiveMinUnitPrice,
                    FiftyMinUnitPrice = c.FiftyMinUnitPrice,
                    ThumbnailUrl = c.ThumbnailUrl,
                    VideoUrl = c.VideoUrl
                };

            return courseManagementData
                .GroupBy(c => c.CourseId)
                .Select(gp => gp.First())
                .ToList();
        }

        public async Task<bool> UpdateCourseInfo(UpdateCourseDto dto)
        {
            //確認連線是否開啟
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            //檢查CourseId是否存在Courses表中
            var courseExists = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Courses WHERE CourseId = @CourseId", new { CourseId = dto.CourseId });

            if (courseExists == 0)
            {
                return false;
            }

            var updateQuery = @"
                UPDATE Courses
                SET Title = @CourseTitle, SubTitle = @CourseSubTitle, CategoryId = @CourseCategory, SubjectId = @CourseSubject, TwentyFiveMinUnitPrice = @TwentyFiveMinUnitPrice, FiftyMinUnitPrice = @FiftyMinUnitPrice, VideoUrl = @VideoUrl, Description = @Description
                WHERE CourseId = @CourseId";

            var deleteImagesQuery = @"DELETE FROM CourseImages WHERE CourseId = @CourseId AND ImageURL NOT IN @CourseImages";

            var insertImagesQuery = @"
                INSERT INTO CourseImages (CourseId, ImageUrl, Cdate)
                VALUES (@CourseId, @ImageUrl, @Cdate)";

            using (var transaction = _dbConnection.BeginTransaction())
            {
                var result = await _dbConnection.ExecuteAsync(updateQuery, dto, transaction);

                //刪除
                await _dbConnection.ExecuteAsync(deleteImagesQuery, new { CourseId = dto.CourseId, CourseImages = dto.CourseImages }, transaction);

                var existingImages = await _dbConnection.QueryAsync<string>(
                "SELECT ImageUrl FROM CourseImages WHERE CourseId = @CourseId",
                new { CourseId = dto.CourseId }, transaction);

                foreach (var imageUrl in dto.CourseImages)
                {
                    if (!existingImages.Contains(imageUrl)) // 檢查圖片是否已存在
                    {
                        await _dbConnection.ExecuteAsync(insertImagesQuery, new { CourseId = dto.CourseId, ImageUrl = imageUrl, Cdate = DateTime.Now }, transaction);
                    }
                }

                transaction.Commit();

                return result > 0;
            }
        }


        public async Task<bool> UpdateCoursesStatus(int courseId, bool courseApprove)
        {
            //檢查CourseId是否存在Courses表中
            var courseExists = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Courses WHERE CourseId = @courseId", new { courseId });

            if (courseExists == 0)
            {
                return false;
            }

            var query = @"
                UPDATE Courses
                SET CoursesStatus = @status, Cdate = @cdate
                WHERE CourseId = @courseId";


            var status = courseApprove ? (short)1 : (short)2;
            var result = await _dbConnection.ExecuteAsync(query, new { status, courseId, cdate = DateTime.Now });
            return result > 0;
        }


        public async Task<bool> UpdatePublishingStatus(int courseId, bool coursePublish)
        {
            var courseExists = await _dbConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Courses WHERE CourseId = @courseId", new { courseId });

            var query = @"
                UPDATE Courses
                SET IsEnabled = @isEnabled, Cdate = @cdate
                WHERE CourseId = @courseId";

            var result = await _dbConnection.ExecuteAsync(query, new { isEnabled = coursePublish, courseId, cdate = DateTime.Now });
            return result > 0;
        }
    }
}
