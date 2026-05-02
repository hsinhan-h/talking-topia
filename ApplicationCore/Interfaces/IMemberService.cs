using ApplicationCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IMemberService
    {
        public Task<bool> GetMemberId(int memberId);
        public bool IsWatched(int memberId, int courseId);
        public int AddWatchList(int memberId, int courseId);
        public bool DeleteWatchList(int memberId, int courseId);
        public bool GetIsTutor(int memberId);
        public Task<SignalRTutorDto> GetTutor(int courseId);
        public Task<SignalRStudentDto> GetStudent(int studentId);
        public Task<string> GetMemberName(int memberId);
    }
}
