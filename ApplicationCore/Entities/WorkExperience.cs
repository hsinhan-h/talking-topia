using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class WorkExperience
{
    /// <summary>
    /// 工作經驗Id
    /// </summary>
    public int WorkExperienceId { get; set; }

    /// <summary>
    /// 工作經驗檔案路徑
    /// </summary>
    public string WorkExperienceFile { get; set; }

    /// <summary>
    /// 工作起始日
    /// </summary>
    public DateOnly WorkStartDate { get; set; }

    /// <summary>
    /// 工作結束日
    /// </summary>
    public DateOnly WorkEndDate { get; set; }

    /// <summary>
    /// 工作經驗名稱
    /// </summary>
    public string WorkName { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual Member Member { get; set; }
}
