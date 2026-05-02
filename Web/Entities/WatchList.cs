using System;
using System.Collections.Generic;

namespace Web.Entities;

public partial class WatchList
{
    /// <summary>
    /// 關注Id
    /// </summary>
    public int WatchListId { get; set; }

    /// <summary>
    /// 送出關注的人
    /// </summary>
    public int? FollowerId { get; set; }

    /// <summary>
    /// 關注的課程
    /// </summary>
    public int? CourseId { get; set; }

    public virtual Course Course { get; set; }

    public virtual Member Follower { get; set; }
}
