using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Dtos
{
    public class SignalRTutorDto
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string HeadShotImage { get; set; }
        public string CourseTitle { get; set; }
        public string CourseSubTitle { get; set; }
    }

    public class SignalRStudentDto
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string HeadShotImage { get; set; }
    }
}
