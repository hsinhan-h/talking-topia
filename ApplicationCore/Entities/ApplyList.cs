using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ApplyList
{
    /// <summary>
    /// 申請Id
    /// </summary>
    public int ApplyId { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 申請狀態
    /// </summary>
    public bool ApplyStatus { get; set; }

    /// <summary>
    /// 申請日期
    /// </summary>
    public DateTime ApplyDateTime { get; set; }

    /// <summary>
    /// 審核通過時間
    /// </summary>
    public DateTime? ApprovedDateTime { get; set; }

    /// <summary>
    /// 更新審核通過時間
    /// </summary>
    public DateTime? UpdateStatusDateTime { get; set; }

    /// <summary>
    /// 拒絕原因
    /// </summary>
    public string RejectReason { get; set; }

    public string AiimageUrl1 { get; set; }

    public string AiimageUrl2 { get; set; }

    public string AiimageUrl3 { get; set; }

    public bool? AiimgageStatus { get; set; }

    public virtual Member Member { get; set; }
}
