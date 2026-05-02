using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Course
{
    /// <summary>
    /// 課程Id
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// 課程類別Id
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 科目Id
    /// </summary>
    public int SubjectId { get; set; }

    /// <summary>
    /// 學生Id
    /// </summary>
    public int TutorId { get; set; }

    /// <summary>
    /// 課程標題
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 課程副標題
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// 25分鐘價
    /// </summary>
    public decimal TwentyFiveMinUnitPrice { get; set; }

    /// <summary>
    /// 50分鐘價
    /// </summary>
    public decimal FiftyMinUnitPrice { get; set; }

    /// <summary>
    /// 課程詳細描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 是否顯示
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 影片封面
    /// </summary>
    public string ThumbnailUrl { get; set; }

    /// <summary>
    /// 影片路徑
    /// </summary>
    public string VideoUrl { get; set; }

    /// <summary>
    /// 課程審核狀態
    /// </summary>
    public short CoursesStatus { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual CourseCategory Category { get; set; }

    public virtual ICollection<CourseImage> CourseImages { get; set; } = new List<CourseImage>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual CourseSubject Subject { get; set; }

    public virtual Member Tutor { get; set; }

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();
}
