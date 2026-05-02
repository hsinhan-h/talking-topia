using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class CourseHour
{
    /// <summary>
    /// 課程時間Id
    /// </summary>
    public int CourseHourId { get; set; }

    /// <summary>
    /// 小時時段
    /// </summary>
    public string Hour { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual ICollection<TutorTimeSlot> TutorTimeSlots { get; set; } = new List<TutorTimeSlot>();
}
