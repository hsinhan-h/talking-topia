using System;
using System.Collections.Generic;

namespace Web.Entities;

public partial class Member
{
    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 會員頭像
    /// </summary>
    public string HeadShotImage { get; set; }

    /// <summary>
    /// 國籍Id
    /// </summary>
    public int? NationId { get; set; }

    /// <summary>
    /// 優質會員
    /// </summary>
    public bool IsVerifiedTutor { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// 姓氏
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 電子郵件信箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 綽號
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    public short Gender { get; set; }

    /// <summary>
    /// 母語
    /// </summary>
    public string NativeLanguage { get; set; }

    /// <summary>
    /// 會的語言
    /// </summary>
    public string SpokenLanguage { get; set; }

    /// <summary>
    /// 銀行代碼
    /// </summary>
    public string BankCode { get; set; }

    /// <summary>
    /// 帳戶名稱
    /// </summary>
    public string BankAccount { get; set; }

    /// <summary>
    /// 最高學歷Id
    /// </summary>
    public int? EducationId { get; set; }

    /// <summary>
    /// 教師自介
    /// </summary>
    public string TutorIntro { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 帳號類型
    /// </summary>
    public int AccountType { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? Udate { get; set; }

    /// <summary>
    /// 是否為教師
    /// </summary>
    public bool IsTutor { get; set; }

    public int? UserId { get; set; }

    public string LineUserId { get; set; }

    public string EmailVerificationToken { get; set; }

    public DateTime EmailVerificationTokenExpiration { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public string ResetPasswordToken { get; set; }

    public virtual ICollection<ApplyCourse> ApplyCourses { get; set; } = new List<ApplyCourse>();

    public virtual ICollection<ApplyList> ApplyLists { get; set; } = new List<ApplyList>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Education Education { get; set; }

    public virtual ICollection<MemberPreference> MemberPreferences { get; set; } = new List<MemberPreference>();

    public virtual Nation Nation { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProfessionalLicense> ProfessionalLicenses { get; set; } = new List<ProfessionalLicense>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<TutorTimeSlot> TutorTimeSlots { get; set; } = new List<TutorTimeSlot>();

    public virtual User User { get; set; }

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();

    public virtual ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
}
