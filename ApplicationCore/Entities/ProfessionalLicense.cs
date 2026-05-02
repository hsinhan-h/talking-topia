using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ProfessionalLicense
{
    /// <summary>
    /// 證照Id
    /// </summary>
    public int ProfessionalLicenseId { get; set; }

    /// <summary>
    /// 證照名稱
    /// </summary>
    public string ProfessionalLicenseName { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 證照路徑
    /// </summary>
    public string ProfessionalLicenseUrl { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual Member Member { get; set; }
}
