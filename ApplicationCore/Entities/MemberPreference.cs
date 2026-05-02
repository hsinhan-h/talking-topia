using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class MemberPreference
{
    /// <summary>
    /// 會員偏好Id
    /// </summary>
    public int MemberPreferenceId { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 主題Id
    /// </summary>
    public int SubjecId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual Member Member { get; set; }

    public virtual CourseSubject Subjec { get; set; }
}
