using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ShoppingCart
{
    /// <summary>
    /// 購物車Id
    /// </summary>
    public int ShoppingCartId { get; set; }

    /// <summary>
    /// 課程Id
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// 課程單價
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 購買堂數
    /// </summary>
    public short Quantity { get; set; }

    /// <summary>
    /// 單筆總價
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 課程類型
    /// </summary>
    public short CourseType { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    /// <summary>
    /// 預約日期
    /// </summary>
    public DateTime? BookingDate { get; set; }

    /// <summary>
    /// 預約時間
    /// </summary>
    public int? BookingTime { get; set; }

    public virtual Course Course { get; set; }

    public virtual Member Member { get; set; }
}
