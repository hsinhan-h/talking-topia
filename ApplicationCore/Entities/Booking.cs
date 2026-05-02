using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Booking
{
    /// <summary>
    /// 預約Id
    /// </summary>
    public int BookingId { get; set; }

    /// <summary>
    /// 課程Id
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// 預約上課日期
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// 預約上課時間
    /// </summary>
    public int BookingTime { get; set; }

    /// <summary>
    /// 預約學生Id
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? Udate { get; set; }

    /// <summary>
    /// 已發送通知次數
    /// </summary>
    public int? NotifyCount { get; set; }

    public virtual Course Course { get; set; }

    public virtual Member Student { get; set; }
}
