using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class CourseMainPageViewModel
    {
        
        public int CourseId { get; set; }
        public int TutorId { get; set; }
        public int CategoryId { get; set; }
        public int MemberId { get; set; }
        [Display(Name = "教師頭像")]
        public string TutorHeadShotImage { get; set; }
        [Display(Name = "教師國旗圖像")]
        public string TutorFlagImage { get; set; }
        [Display(Name = "優良教師")]
        public bool IsVerifiedTutor { get; set; }
        [Display(Name = "會說的語言")]
        public string SpokenLanguage { get; set; }
        public int EducationId { get; set; }
        [Display(Name = "學歷")]
        public List<TutorEducationList> EducationDegree { get; set; }
        [Display(Name = "經歷")]
        public List<TutorExperience> ExperienceList { get; set; }
        public List<TutorProfessionList> ProfessionList { get; set; }
        [Display(Name = "課程名稱")]
        public string CourseTitle { get; set; }
        [Display(Name = "課程副標題")]
        public string CourseSubTitle { get; set; }
        [Display(Name = "教師簡介")]
        public string TutorIntro { get; set; }
        [Display(Name = "課程介紹")]
        public string CourseIntro { get; set; }
        [Display(Name = "25分鐘台幣價格")]
        public int TwentyFiveMinPrice { get; set; }
        public List<BaseDiscountPice> TwentyFiveDiscountedPrice { get; set; }

        [Display(Name = "50分鐘台幣價格")]
        public int FiftyMinPrice { get; set; }
        
        public List<BaseDiscountPice> FiftyDiscountedPrice { get; set; }

        [Display(Name = "課程影片")]
        public string CourseVideo { get; set; }
        [Display(Name = "課程影片縮圖")]
        public string CourseVideoThumbnail { get; set; }
        [Display(Name = "課程照片")]
        public List<CourseImageViewModel> CourseImages { get; set; }
        [Display(Name = "完成課堂數")]
        public int FinishedCoursesTotal { get; set; }
        [Display(Name = "已被預約時段")]
        public List<TimeSlotViewModel> BookedTimeSlots { get; set; }
        [Display(Name = "可預約時段")]
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; }
        [Display(Name = "課程評分")]
        public double CourseRatings { get; set; }
        [Display(Name = "課程評論數")]
        public int CourseReviews { get; set; }
        public List<ReviewViewModel> ReviewCardList { get; set; }
        [Display(Name = "關注狀態")]
        public bool FollowingStatus { get; set; }
        public List<TutorRecomCardList> TutorReconmmendCard { get; set; }
        
        public List<TutorRecomCardList> LanguageWatchList { get; set; }
        public List<TutorRecomCardList> CodingWatchList { get; set; }
        public List<TutorRecomCardList> SchoolWatchList { get; set; }


        [Required(ErrorMessage = "請輸入評論內容")]
        [StringLength(500, ErrorMessage = "評論內容不可超過500個字")]
        [Display(Name = "評論內容")]
        public string NewReviewContent { get; set; }
    }

    public class TutorExperience 
    {
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public string WorkTitle { get; set; }
    }
    public class TutorProfessionList
    { 
        public string ProfessionName { get; set; }
    }
    

    public class ReviewViewModel
    {
        public double CommentRating { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewDate { get; set; }
        public string ReviewContent { get; set; }
    }

    public class TutorEducationList
    { 
        public int StudyStartYear { get; set; }
        public int StudyEndYear { get; set; }
        public string SchoolAndDepartment { get; set; }
    }
    
    public class BaseDiscountPice
    {
       public int CourseCount { get; set; }
        public int CourseDurance { get; set; }

        public decimal Discount {  get; set; }

        public string DiscountPrice { get; set; }
    }

    public class CourseCountDiscount
    {
        public int CourseCount { get; set; }
        public decimal Discount { get; set; }
    }

    public class TutorRecomCardList
    {
        public int CourseId { get; set; }
        public int CategoryId { get; set; }
        public string TutorHeadShot { get; set; }
        public string NationFlagImg { get; set; }
        public string CourseTitle { get; set; }
        public string CourseSubTitle { get; set; }
        public string TwentyFiveMinPrice { get; set; }
        public string FiftyminPrice { get; set; }
        public double Rating { get; set; }
        public string Description { get; set; }
    }
}
