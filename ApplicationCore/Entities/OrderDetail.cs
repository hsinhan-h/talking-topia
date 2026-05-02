using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class OrderDetail
{
    /// <summary>
    /// 訂單明細Id
    /// </summary>
    public int OrderDetailId { get; set; }

    /// <summary>
    /// 訂單Id
    /// </summary>
    public int OrderId { get; set; }

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
    /// 折扣金額
    /// </summary>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// 總價
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 課程類別
    /// </summary>
    public short CourseType { get; set; }

    /// <summary>
    /// 課程名稱
    /// </summary>
    public string CourseTitle { get; set; }

    /// <summary>
    /// 課程類別
    /// </summary>
    public string CourseCategory { get; set; }

    /// <summary>
    /// 課程主題
    /// </summary>
    public string CourseSubject { get; set; }

    public virtual Course Course { get; set; }

    public virtual Order Order { get; set; }
}
