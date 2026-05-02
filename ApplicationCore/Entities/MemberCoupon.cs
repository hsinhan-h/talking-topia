using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class MemberCoupon
{
    /// <summary>
    /// 會員優惠Id
    /// </summary>
    public int MemberCouponId { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 是否使用
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// 優惠折扣Id
    /// </summary>
    public int CouponId { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    public virtual Coupon Coupon { get; set; }

    public virtual Member Member { get; set; }
}
