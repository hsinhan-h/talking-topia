using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ApplyCourse
{
    public int ApplyCourseId { get; set; }

    public int MemberId { get; set; }

    public int ApplyCourseCategoryId { get; set; }

    public int ApplySubCategoryId { get; set; }

    public DateTime? Udate { get; set; }

    public DateTime Cdate { get; set; }

    public virtual ApplyCourseCategory ApplyCourseCategory { get; set; }

    public virtual ApplyCourseSubCategory ApplySubCategory { get; set; }

    public virtual Member Member { get; set; }
}
