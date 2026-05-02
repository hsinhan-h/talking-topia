using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class CourseImage
{
    /// <summary>
    /// 課程照片Id
    /// </summary>
    public int CourseImageId { get; set; }

    /// <summary>
    /// 課程Id
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// 圖片路徑
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Cdate { get; set; }

    /// <summary>
    /// 更改時間
    /// </summary>
    public DateTime? Udate { get; set; }

    public virtual Course Course { get; set; }
}
