using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Review
{
    /// <summary>
    /// 課程評論Id
    /// </summary>
    public int ReviewId { get; set; }

    /// <summary>
    /// 學生Id
    /// </summary>
    public int StudentId { get; set; }

    /// <summary>
    /// 課程Id
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// 評分
    /// </summary>
    public byte Rating { get; set; }

    /// <summary>
    /// 評論內容
    /// </summary>
    public string CommentText { get; set; }

    /// <summary>
    /// 評論日期
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual Course Course { get; set; }

    public virtual Member Student { get; set; }
}
