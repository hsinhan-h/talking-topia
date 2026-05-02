using Api.Services;

namespace Api.Dtos
{
    public class TutorDataDto
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string MemberName { get; set; }
        public string Istutor { get; set; }
        public bool ApplyStatus { get; set; }

        public string ApplyDateTime { get; set; }

        public string ApprovedDateTime { get; set; }

        public string RejectReason { get; set; }
        public string ResumeStatus { get; set; }
        public string RequestAI { get; set; }
    }
    public class UpdateTutorDataDto
    {
        public int MemberId { get; set; }
        public bool Istutor { get; set; }
        public bool ApplyStatus { get; set; }

        public string ApprovedDateTime { get; set; }

        public string? RejectReason { get; set; }
        //public string ResumeStatus { get; set; }
    }
    public class TutorDataStatisticsDto
    {
        public int MemberCount { get; set; }
        public int IsTutorCount { get; set; }  
        public int ApplyCount { get; set; }     
        public int PendingReviewCount { get; set; }
        public int MonthlyApplyCount { get; set; } 
        public int MonthlyPendingReviewCount { get; set; } 
        public int MonthlyIsTutorCount { get; set; }
        public int MonthlyNewMemberCount { get; set; }
        public string CurrentMonth { get; set; }
        
    }
    public class TutorHeadImgDto
    {
        public int MemberId { get; set; }
        public string HeadImgUrl { get; set; }

        public string FileName { get; set; }
    }
    public class UpdateImageDto
    {
        public int MemberId { get; set; }
        public List<string> ImageUrls { get; set; }
    }

}
