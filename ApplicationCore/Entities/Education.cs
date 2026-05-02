using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Education
{
    /// <summary>
    /// 學歷Id
    /// </summary>
    public int EducationId { get; set; }

    /// <summary>
    /// 學校名稱
    /// </summary>
    public string SchoolName { get; set; }

    /// <summary>
    /// 在學期間起
    /// </summary>
    public int StudyStartYear { get; set; }

    /// <summary>
    /// 在學期間迄
    /// </summary>
    public int StudyEndYear { get; set; }

    /// <summary>
    /// 科系名稱
    /// </summary>
    public string DepartmentName { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
