using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Nation
{
    /// <summary>
    /// 國籍Id
    /// </summary>
    public int NationId { get; set; }

    /// <summary>
    /// 國籍名稱
    /// </summary>
    public string NationName { get; set; }

    /// <summary>
    /// 國籍圖片路徑
    /// </summary>
    public string FlagImage { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
