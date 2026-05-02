using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class SearchService : ISearchService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<CourseSubject> _courseSubjectRepository;
        private readonly IRepository<CourseCategory> _courseCategoryRepository;

        public SearchService(IRepository<Course> courseRepository, IRepository<CourseSubject> courseSubjectRepository, IRepository<CourseCategory> courseCategoryRepository)
        {
            _courseRepository = courseRepository;
            _courseSubjectRepository = courseSubjectRepository;
            _courseCategoryRepository = courseCategoryRepository;
        }

        public async Task<List<Course>> SearchCoursesAsync(string query)
        {
            // 獲取所有的課程資料（此查詢應該可以被翻譯為 SQL）
            var courses = await _courseRepository.ListAsync(c => c.Title.Contains(query) || c.SubTitle.Contains(query));

            // 獲取符合條件的科目，並使用 .AsEnumerable() 來進行客戶端評估
            var courseSubjects = (await _courseSubjectRepository.ListAsync()).AsEnumerable()
                .Where(s => s.SubjectName.Contains(query))
                .ToList();

            // 獲取與科目相關的課程
            var relatedCoursesBySubjectIds = courseSubjects.Select(s => s.SubjectId).ToList();
            var relatedCoursesBySubject = (await _courseRepository.ListAsync()).AsEnumerable()
                .Where(c => relatedCoursesBySubjectIds.Contains(c.SubjectId))
                .ToList();

            // 合併所有查詢結果並移除重複
            var result = courses
                .Union(relatedCoursesBySubject)
                .Distinct()
                .ToList();

            return result;
        }
    }
}
