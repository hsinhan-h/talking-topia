using ApplicationCore.Dtos;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public class MemberService : IMemberService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<WatchList> _watchListRepository;
        private readonly IRepository<Course> _courseRepository;

        public MemberService(IRepository<Member> memberRepository, IRepository<WatchList> watchListRepository, IRepository<Course> courseRepository)
        {
            _memberRepository = memberRepository;
            _watchListRepository = watchListRepository;
            _courseRepository = courseRepository;
        }

        public async Task<bool> GetMemberId(int memberId)
        {
            var result = await _memberRepository.AnyAsync(x => x.MemberId == memberId);            
            return result;
        }

        public bool IsWatched(int memberId, int courseId)
        {
            var IsFollowed = _watchListRepository.List().Any(w => w.CourseId == courseId && w.FollowerId == memberId);
            return IsFollowed;
        }

        public int AddWatchList(int memberId, int courseId)
        {
            try 
            {
                var addFollow = new WatchList
                {                    
                    CourseId = courseId,
                    FollowerId = memberId,
                };
                var addWatchList = _watchListRepository.Add(addFollow);
                //if (addWatchList is null)
                //{
                //    throw new Exception("WatchList could not be created");
                //}
                return addWatchList.WatchListId;
            }
            catch (Exception ex)
            {
                throw new Exception("WatchList could not be created", ex);
            }

        }

        public bool DeleteWatchList(int memberId, int courseId) 
        {
            var findWatchList = _watchListRepository.FirstOrDefault(w => w.FollowerId == memberId && w.CourseId == courseId);
            if (findWatchList != null)
            {
               _watchListRepository.Delete(findWatchList);
                return true;
            }
            else 
            {
                throw new Exception("WatchList could not be Deleted");
            }
            
        }

        public bool GetIsTutor(int memberId)
        {
            var result =  _memberRepository.FirstOrDefault(x => x.MemberId == memberId).IsTutor;
            return result;
        }

        public async Task<SignalRTutorDto> GetTutor(int courseId)
        {
            var course = await _courseRepository.FirstOrDefaultAsync(c => c.CourseId == courseId);
            var tutor = await _memberRepository.FirstOrDefaultAsync(x => x.MemberId == course.TutorId);
            var result = new SignalRTutorDto
            {
                MemberId = tutor.MemberId,
                MemberName = tutor.FirstName,
                HeadShotImage = tutor.HeadShotImage,
                CourseTitle = course.Title,
                CourseSubTitle = course.SubTitle,
            };
            return result;
        }

        public async Task<string> GetMemberName(int memberId)
        {
            var result = await _memberRepository.FirstOrDefaultAsync(x => x.MemberId == memberId);
            return result.FirstName;
        }

        public async Task<SignalRStudentDto> GetStudent(int studentId)
        {
            var student = await _memberRepository.FirstOrDefaultAsync(x => x.MemberId == studentId);
            var result = new SignalRStudentDto
            {
                MemberId= student.MemberId,
                MemberName= student.FirstName,
                HeadShotImage= student.HeadShotImage,
            };
            return result;
        }
    }
}
