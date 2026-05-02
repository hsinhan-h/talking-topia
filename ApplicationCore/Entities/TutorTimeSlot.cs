using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;
public partial class TutorTimeSlot
{
    /// <summary>
    /// 教師可預約Id
    /// </summary>
    public int TutorTimeSlotId { get; set; }

    /// <summary>
    /// 老師Id
    /// </summary>
    public int TutorId { get; set; }

    /// <summary>
    /// 開課星期
    /// </summary>
    public int Weekday { get; set; }

    /// <summary>
    /// 開課時間
    /// </summary>
    public int CourseHourId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual CourseHour CourseHour { get; set; }

    public virtual Member Tutor { get; set; }
}
