using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ApplicationCore.Entities;
namespace Web.Data;

public partial class TalkingTopiaDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public TalkingTopiaDbContext(DbContextOptions<TalkingTopiaDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    public virtual DbSet<Entities.ApplyCourse> ApplyCourses { get; set; }

    public virtual DbSet<Entities.ApplyCourseCategory> ApplyCourseCategories { get; set; }

    public virtual DbSet<Entities.ApplyCourseSubCategory> ApplyCourseSubCategories { get; set; }

    public virtual DbSet<Entities.ApplyList> ApplyLists { get; set; }

    public virtual DbSet<Entities.Booking> Bookings { get; set; }

    public virtual DbSet<Entities.Coupon> Coupons { get; set; }

    public virtual DbSet<Entities.Course> Courses { get; set; }

    public virtual DbSet<Entities.CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<Entities.CourseHour> CourseHours { get; set; }

    public virtual DbSet<Entities.CourseImage> CourseImages { get; set; }

    public virtual DbSet<Entities.CourseSubject> CourseSubjects { get; set; }

    public virtual DbSet<Entities.Education> Educations { get; set; }

    public virtual DbSet<Entities.Member> Members { get; set; }

    public virtual DbSet<Entities.MemberCoupon> MemberCoupons { get; set; }

    public virtual DbSet<Entities.MemberPreference> MemberPreferences { get; set; }

    public virtual DbSet<Entities.Nation> Nations { get; set; }

    public virtual DbSet<Entities.Order> Orders { get; set; }

    public virtual DbSet<Entities.OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Entities.ProfessionalLicense> ProfessionalLicenses { get; set; }

    public virtual DbSet<Entities.Review> Reviews { get; set; }

    public virtual DbSet<Entities.Role> Roles { get; set; }

    public virtual DbSet<Entities.ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<Entities.TutorTimeSlot> TutorTimeSlots { get; set; }

    public virtual DbSet<Entities.User> Users { get; set; }

    public virtual DbSet<Entities.UserRole> UserRoles { get; set; }

    public virtual DbSet<Entities.WatchList> WatchLists { get; set; }

    public virtual DbSet<Entities.WorkExperience> WorkExperiences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:bs-2024-summer-03.database.windows.net,1433;Initial Catalog=TalkingTopiaDb;Persist Security Info=False;User ID=bs;Password=P@ssword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

        modelBuilder.Entity<Entities.ApplyCourse>(entity =>
        {
            entity.Property(e => e.Cdate)
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.Udate)
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.ApplyCourseCategory).WithMany(p => p.ApplyCourses)
                .HasForeignKey(d => d.ApplyCourseCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplyCourses_ApplyCourseCategory");

            entity.HasOne(d => d.ApplySubCategory).WithMany(p => p.ApplyCourses)
                .HasForeignKey(d => d.ApplySubCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplyCourses_ApplyCourseSubCategory");

            entity.HasOne(d => d.Member).WithMany(p => p.ApplyCourses)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplyCourses_Members");
        });

        modelBuilder.Entity<Entities.ApplyCourseCategory>(entity =>
        {
            entity.HasKey(e => e.ApplyCourseCategoryId).HasName("PK__ApplyCou__1CC616FF4D15312C");

            entity.ToTable("ApplyCourseCategory");

            entity.Property(e => e.ApplyCategoryName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Cdate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.Udate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
        });

        modelBuilder.Entity<Entities.ApplyCourseSubCategory>(entity =>
        {
            entity.HasKey(e => e.ApplySubCategoryId).HasName("PK__ApplyCou__EBF7B0ADD349BDC6");

            entity.ToTable("ApplyCourseSubCategory");

            entity.Property(e => e.ApplySubCategoryId).ValueGeneratedNever();
            entity.Property(e => e.ApplySubCategoryName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Cdate)
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.Udate)
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.ApplyCourseCategory).WithMany(p => p.ApplyCourseSubCategories)
                .HasForeignKey(d => d.ApplyCourseCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplyCourseSubCategory_ApplyCourseCategory1");
        });

        modelBuilder.Entity<Entities.ApplyList>(entity =>
        {
            entity.HasKey(e => e.ApplyId).HasName("PK__ApplyLis__F0687F91F95B14E5");

            entity.Property(e => e.ApplyId)
                .HasComment("申請Id")
                .HasColumnName("ApplyID");
            entity.Property(e => e.AiimageUrl1).HasColumnName("AIImageUrl1");
            entity.Property(e => e.AiimageUrl2).HasColumnName("AIImageUrl2");
            entity.Property(e => e.AiimageUrl3).HasColumnName("AIImageUrl3");
            entity.Property(e => e.AiimgageStatus).HasColumnName("AIImgageStatus");
            entity.Property(e => e.ApplyDateTime)
                .HasComment("申請日期")
                .HasColumnType("datetime");
            entity.Property(e => e.ApplyStatus).HasComment("申請狀態");
            entity.Property(e => e.ApprovedDateTime)
                .HasComment("審核通過時間")
                .HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(50)
                .HasComment("拒絕原因");
            entity.Property(e => e.UpdateStatusDateTime)
                .HasComment("更新審核通過時間")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.ApplyLists)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplyLists_Members");
        });

        modelBuilder.Entity<Entities.Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AEDF4836C80");

            entity.HasIndex(e => e.CourseId, "IX_Bookings_CourseId");

            entity.HasIndex(e => e.StudentId, "IX_Bookings_StudentId");

            entity.Property(e => e.BookingId).HasComment("預約Id");
            entity.Property(e => e.BookingDate).HasComment("預約上課日期");
            entity.Property(e => e.BookingTime).HasComment("預約上課時間");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.NotifyCount).HasComment("已發送通知次數");
            entity.Property(e => e.StudentId).HasComment("預約學生Id");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Course).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Courses");

            entity.HasOne(d => d.Student).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Members");
        });

        modelBuilder.Entity<Entities.Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupons__384AF1BAE1D06BB9");

            entity.Property(e => e.CouponId).HasComment("優惠折扣Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CouponCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("折扣代碼");
            entity.Property(e => e.CouponName)
                .IsRequired()
                .HasMaxLength(50)
                .IsFixedLength()
                .HasComment("優惠折扣名稱");
            entity.Property(e => e.Discount).HasComment("折扣");
            entity.Property(e => e.DiscountType).HasComment("折扣方式");
            entity.Property(e => e.ExpirationDate)
                .HasComment("折扣到期日")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasComment("是否有效");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
        });

        modelBuilder.Entity<Entities.Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7F51F70E3");

            entity.HasIndex(e => e.CategoryId, "IX_Courses_CategoryId");

            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.CategoryId).HasComment("課程類別Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CoursesStatus).HasComment("課程審核狀態");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasComment("課程詳細描述");
            entity.Property(e => e.FiftyMinUnitPrice)
                .HasComment("50分鐘價")
                .HasColumnType("money");
            entity.Property(e => e.IsEnabled).HasComment("是否顯示");
            entity.Property(e => e.SubTitle)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("課程副標題");
            entity.Property(e => e.SubjectId).HasComment("科目Id");
            entity.Property(e => e.ThumbnailUrl)
                .IsRequired()
                .HasComment("影片封面");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("課程標題");
            entity.Property(e => e.TutorId).HasComment("學生Id");
            entity.Property(e => e.TwentyFiveMinUnitPrice)
                .HasComment("25分鐘價")
                .HasColumnType("money");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
            entity.Property(e => e.VideoUrl)
                .IsRequired()
                .HasComment("影片路徑");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_CourseCategories");

            entity.HasOne(d => d.Subject).WithMany(p => p.Courses)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_CourseSubjects");

            entity.HasOne(d => d.Tutor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Tutors");
        });

        modelBuilder.Entity<Entities.CourseCategory>(entity =>
        {
            entity.HasKey(e => e.CourseCategoryId).HasName("PK__CourseCa__4D67EBB68E28BA31");

            entity.Property(e => e.CourseCategoryId).HasComment("課程類別Id");
            entity.Property(e => e.CategorytName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("課程類別名稱");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
        });

        modelBuilder.Entity<Entities.CourseHour>(entity =>
        {
            entity.HasKey(e => e.CourseHourId).HasName("PK__CourseHo__AE73575BBC30FF2E");

            entity.Property(e => e.CourseHourId).HasComment("課程時間Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.Hour)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("小時時段");
            entity.Property(e => e.Udate)
                .HasComment("更改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
        });

        modelBuilder.Entity<Entities.CourseImage>(entity =>
        {
            entity.HasKey(e => e.CourseImageId).HasName("PK__CourseIm__349B6FE480594337");

            entity.HasIndex(e => e.CourseId, "IX_CourseImages_CourseId");

            entity.Property(e => e.CourseImageId).HasComment("課程照片Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasComment("圖片路徑");
            entity.Property(e => e.Udate)
                .HasComment("更改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseImages)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseIma__Cours__52593CB8");
        });

        modelBuilder.Entity<Entities.CourseSubject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__CourseSu__AC1BA3A8B5819935");

            entity.HasIndex(e => e.CourseCategoryId, "IX_CourseSubjects_CourseCategoryId");

            entity.Property(e => e.SubjectId).HasComment("課程科目Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CourseCategoryId).HasComment("課程類別Id");
            entity.Property(e => e.SubjectName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("課程科目名稱");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.CourseCategory).WithMany(p => p.CourseSubjects)
                .HasForeignKey(d => d.CourseCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSub__Cours__59063A47");
        });

        modelBuilder.Entity<Entities.Education>(entity =>
        {
            entity.HasKey(e => e.EducationId).HasName("PK__Educatio__4BBE38058A56247B");

            entity.Property(e => e.EducationId).HasComment("學歷Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .HasComment("科系名稱");
            entity.Property(e => e.SchoolName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("學校名稱");
            entity.Property(e => e.StudyEndYear).HasComment("在學期間迄");
            entity.Property(e => e.StudyStartYear).HasComment("在學期間起");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
        });

        modelBuilder.Entity<Entities.Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Members__0CF04B1808627D7C");

            entity.HasIndex(e => e.EducationId, "IX_Members_EducationId");

            entity.HasIndex(e => e.NationId, "IX_Members_NationId");

            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.Account).HasComment("帳號");
            entity.Property(e => e.AccountType).HasComment("帳號類型");
            entity.Property(e => e.BankAccount)
                .HasMaxLength(50)
                .HasComment("帳戶名稱");
            entity.Property(e => e.BankCode)
                .HasMaxLength(50)
                .HasComment("銀行代碼");
            entity.Property(e => e.Birthday)
                .HasComment("生日")
                .HasColumnType("datetime");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.EducationId).HasComment("最高學歷Id");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("電子郵件信箱");
            entity.Property(e => e.EmailVerificationToken)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.EmailVerificationTokenExpiration)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("名字");
            entity.Property(e => e.Gender).HasComment("性別");
            entity.Property(e => e.HeadShotImage).HasComment("會員頭像");
            entity.Property(e => e.IsTutor).HasComment("是否為教師");
            entity.Property(e => e.IsVerifiedTutor).HasComment("優質會員");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("姓氏");
            entity.Property(e => e.LineUserId).HasMaxLength(50);
            entity.Property(e => e.NationId).HasComment("國籍Id");
            entity.Property(e => e.NativeLanguage)
                .HasMaxLength(255)
                .HasComment("母語");
            entity.Property(e => e.Nickname)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("綽號");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("密碼");
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("電話");
            entity.Property(e => e.ResetPasswordToken).HasMaxLength(256);
            entity.Property(e => e.SpokenLanguage)
                .HasMaxLength(255)
                .HasComment("會的語言");
            entity.Property(e => e.TutorIntro).HasComment("教師自介");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Education).WithMany(p => p.Members)
                .HasForeignKey(d => d.EducationId)
                .HasConstraintName("FK__Members__Educati__49C3F6B7");

            entity.HasOne(d => d.Nation).WithMany(p => p.Members)
                .HasForeignKey(d => d.NationId)
                .HasConstraintName("FK__Members__NationI__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.Members).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Entities.MemberCoupon>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.CouponId, "IX_MemberCoupons_CouponId");

            entity.HasIndex(e => e.MemberId, "IX_MemberCoupons_MemberId");

            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CouponId).HasComment("優惠折扣Id");
            entity.Property(e => e.IsUsed).HasComment("是否使用");
            entity.Property(e => e.MemberCouponId).HasComment("會員優惠Id");
            entity.Property(e => e.MemberId).HasComment("會員Id");

            entity.HasOne(d => d.Coupon).WithMany()
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberCoupons_Coupons");

            entity.HasOne(d => d.Member).WithMany()
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberCoupons_Members");
        });

        modelBuilder.Entity<Entities.MemberPreference>(entity =>
        {
            entity.HasKey(e => e.MemberPreferenceId).HasName("PK__MemberPr__5B2A2D7058311916");

            entity.HasIndex(e => e.MemberId, "IX_MemberPreferences_MemberId");

            entity.HasIndex(e => e.SubjecId, "IX_MemberPreferences_SubjecId");

            entity.Property(e => e.MemberPreferenceId).HasComment("會員偏好Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.SubjecId).HasComment("主題Id");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberPreferences)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberPre__Membe__5AEE82B9");

            entity.HasOne(d => d.Subjec).WithMany(p => p.MemberPreferences)
                .HasForeignKey(d => d.SubjecId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberPre__Subje__52593CB8");
        });

        modelBuilder.Entity<Entities.Nation>(entity =>
        {
            entity.HasKey(e => e.NationId).HasName("PK__Nations__211B9BBEE3B01F5C");

            entity.Property(e => e.NationId).HasComment("國籍Id");
            entity.Property(e => e.FlagImage)
                .IsRequired()
                .HasComment("國籍圖片路徑");
            entity.Property(e => e.NationName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("國籍名稱");
        });

        modelBuilder.Entity<Entities.Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFBCC07793");

            entity.HasIndex(e => e.MemberId, "IX_Orders_MemberId");

            entity.Property(e => e.OrderId).HasComment("訂單Id");
            entity.Property(e => e.CouponPrice)
                .HasComment("優惠金額")
                .HasColumnType("money");
            entity.Property(e => e.InvoiceType).HasComment("發票類型");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.MerchantTradeNo).HasMaxLength(50);
            entity.Property(e => e.OrderStatusId).HasComment("訂單狀態");
            entity.Property(e => e.PaymentType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("付款方式");
            entity.Property(e => e.SentVatemail)
                .HasMaxLength(100)
                .HasComment("寄送Mail")
                .HasColumnName("SentVATEmail");
            entity.Property(e => e.TaxIdNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("統一編號");
            entity.Property(e => e.TotalPrice)
                .HasComment("總金額")
                .HasColumnType("money");
            entity.Property(e => e.TradeNo).HasMaxLength(50);
            entity.Property(e => e.TransactionDate)
                .HasComment("交易日期")
                .HasColumnType("datetime");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
            entity.Property(e => e.Vatnumber)
                .HasMaxLength(8)
                .HasComment("發票號碼")
                .HasColumnName("VATNumber");

            entity.HasOne(d => d.Member).WithMany(p => p.Orders)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__MemberId__4BAC3F29");
        });

        modelBuilder.Entity<Entities.OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderDetailId, e.OrderId }).HasName("PK__OrderDet__3F80D6D0305DA525");

            entity.HasIndex(e => e.CourseId, "IX_OrderDetails_CourseId");

            entity.HasIndex(e => e.OrderId, "IX_OrderDetails_OrderId");

            entity.Property(e => e.OrderDetailId)
                .ValueGeneratedOnAdd()
                .HasComment("訂單明細Id");
            entity.Property(e => e.OrderId).HasComment("訂單Id");
            entity.Property(e => e.CourseCategory)
                .HasMaxLength(50)
                .HasComment("課程類別");
            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.CourseSubject)
                .HasMaxLength(50)
                .HasComment("課程主題");
            entity.Property(e => e.CourseTitle)
                .HasMaxLength(255)
                .HasComment("課程名稱");
            entity.Property(e => e.CourseType).HasComment("課程類別");
            entity.Property(e => e.DiscountPrice)
                .HasComment("折扣金額")
                .HasColumnType("money");
            entity.Property(e => e.Quantity).HasComment("購買堂數");
            entity.Property(e => e.TotalPrice)
                .HasComment("總價")
                .HasColumnType("money");
            entity.Property(e => e.UnitPrice)
                .HasComment("課程單價")
                .HasColumnType("money");

            entity.HasOne(d => d.Course).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Cours__4E88ABD4");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__4D94879B");
        });

        modelBuilder.Entity<Entities.ProfessionalLicense>(entity =>
        {
            entity.HasKey(e => e.ProfessionalLicenseId).HasName("PK__Professi__E1630CEE26905146");

            entity.HasIndex(e => e.MemberId, "IX_ProfessionalLicenses_MemberId");

            entity.Property(e => e.ProfessionalLicenseId).HasComment("證照Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.ProfessionalLicenseName)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("證照名稱");
            entity.Property(e => e.ProfessionalLicenseUrl)
                .IsRequired()
                .HasComment("證照路徑");
            entity.Property(e => e.Udate)
                .HasComment("更新時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Member).WithMany(p => p.ProfessionalLicenses)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Professio__Membe__5165187F");
        });

        modelBuilder.Entity<Entities.Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE821ED086");

            entity.HasIndex(e => e.CourseId, "IX_Reviews_CourseId");

            entity.HasIndex(e => e.StudentId, "IX_Reviews_StudentId");

            entity.Property(e => e.ReviewId).HasComment("課程評論Id");
            entity.Property(e => e.Cdate)
                .HasComment("評論日期")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CommentText).HasComment("評論內容");
            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.Rating).HasComment("評分");
            entity.Property(e => e.StudentId).HasComment("學生Id");
            entity.Property(e => e.Udate)
                .HasComment("修改日期")
                .HasColumnType("datetime")
                .HasColumnName("UDate");

            entity.HasOne(d => d.Course).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__CourseI__5070F446");

            entity.HasOne(d => d.Student).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__MemberI__4F7CD00D");
        });

        modelBuilder.Entity<Entities.ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.ShoppingCartId).HasName("PK__TempOrde__38D216B780E2926D");

            entity.HasIndex(e => e.CourseId, "IX_ShoppingCarts_CourseId");

            entity.HasIndex(e => e.MemberId, "IX_ShoppingCarts_MemberId");

            entity.Property(e => e.ShoppingCartId).HasComment("購物車Id");
            entity.Property(e => e.BookingDate)
                .HasComment("預約日期")
                .HasColumnType("datetime");
            entity.Property(e => e.BookingTime).HasComment("預約時間");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CourseId).HasComment("課程Id");
            entity.Property(e => e.CourseType).HasComment("課程類型");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.Quantity).HasComment("購買堂數");
            entity.Property(e => e.TotalPrice)
                .HasComment("單筆總價")
                .HasColumnType("money");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
            entity.Property(e => e.UnitPrice)
                .HasComment("課程單價")
                .HasColumnType("money");

            entity.HasOne(d => d.Course).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCarts_Courses");

            entity.HasOne(d => d.Member).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCarts_Members");
        });

        modelBuilder.Entity<Entities.TutorTimeSlot>(entity =>
        {
            entity.HasKey(e => e.TutorTimeSlotId).HasName("PK__TutorTim__E709EE17B13CB862");

            entity.HasIndex(e => e.CourseHourId, "IX_TutorTimeSlots_CourseHourId");

            entity.HasIndex(e => e.TutorId, "IX_TutorTimeSlots_TutorID");

            entity.Property(e => e.TutorTimeSlotId).HasComment("教師可預約Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.CourseHourId).HasComment("開課時間");
            entity.Property(e => e.TutorId)
                .HasComment("老師Id")
                .HasColumnName("TutorID");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
            entity.Property(e => e.Weekday).HasComment("開課星期");

            entity.HasOne(d => d.CourseHour).WithMany(p => p.TutorTimeSlots)
                .HasForeignKey(d => d.CourseHourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TutorTime__Cours__5EBF139D");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorTimeSlots)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TutorTime__Membe__5535A963");
        });

        modelBuilder.Entity<Entities.User>(entity =>
        {
            entity.Property(e => e.LineId).HasMaxLength(255);
        });

        modelBuilder.Entity<Entities.UserRole>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.HasIndex(e => e.UserId, "IX_UserRoles_UserId");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Entities.WatchList>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_WatchLists_CourseId");

            entity.Property(e => e.WatchListId).HasComment("關注Id");
            entity.Property(e => e.CourseId).HasComment("關注的課程");
            entity.Property(e => e.FollowerId).HasComment("送出關注的人");

            entity.HasOne(d => d.Course).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_WatchLists_Courses");

            entity.HasOne(d => d.Follower).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.FollowerId)
                .HasConstraintName("FK_WatchLists_Members");
        });

        modelBuilder.Entity<Entities.WorkExperience>(entity =>
        {
            entity.Property(e => e.WorkExperienceId).HasComment("工作經驗Id");
            entity.Property(e => e.Cdate)
                .HasComment("建立時間")
                .HasColumnType("datetime")
                .HasColumnName("CDate");
            entity.Property(e => e.MemberId).HasComment("會員Id");
            entity.Property(e => e.Udate)
                .HasComment("修改時間")
                .HasColumnType("datetime")
                .HasColumnName("UDate");
            entity.Property(e => e.WorkEndDate).HasComment("工作結束日");
            entity.Property(e => e.WorkExperienceFile).HasComment("工作經驗檔案路徑");
            entity.Property(e => e.WorkName)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("工作經驗名稱");
            entity.Property(e => e.WorkStartDate).HasComment("工作起始日");

            entity.HasOne(d => d.Member).WithMany(p => p.WorkExperiences)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkExperiences_Members");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
