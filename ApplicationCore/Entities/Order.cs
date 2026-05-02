using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Order
{
    /// <summary>
    /// 訂單Id
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 會員Id
    /// </summary>
    public int MemberId { get; set; }

    /// <summary>
    /// 付款方式
    /// </summary>
    public string PaymentType { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 交易日期
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// 優惠金額
    /// </summary>
    public decimal? CouponPrice { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    public string TaxIdNumber { get; set; }

    /// <summary>
    /// 發票類型
    /// </summary>
    public short InvoiceType { get; set; }

    /// <summary>
    /// 發票號碼
    /// </summary>
    public string Vatnumber { get; set; }

    /// <summary>
    /// 寄送Mail
    /// </summary>
    public string SentVatemail { get; set; }

    /// <summary>
    /// 訂單狀態
    /// </summary>
    public short OrderStatusId { get; set; }

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public string MerchantTradeNo { get; set; }

    public string TradeNo { get; set; }

    public virtual Member Member { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
