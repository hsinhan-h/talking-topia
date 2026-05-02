using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class CourseSubject
{
    /// <summary>
    /// 課程科目Id
    /// </summary>
    public int SubjectId { get; set; }

    /// <summary>
    /// 課程科目名稱
    /// </summary>
    public string SubjectName { get; set; }

    /// <summary>
    /// 課程類別Id
    /// </summary>
    public int CourseCategoryId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual CourseCategory CourseCategory { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<MemberPreference> MemberPreferences { get; set; } = new List<MemberPreference>();
}
