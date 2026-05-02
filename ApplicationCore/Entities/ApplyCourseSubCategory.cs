using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ApplyCourseSubCategory
{
    public int ApplySubCategoryId { get; set; }

    public string ApplySubCategoryName { get; set; }

    public int ApplyCourseCategoryId { get; set; }

    public DateTime Cdate { get; set; }

    public DateTime? Udate { get; set; }

    public virtual ApplyCourseCategory ApplyCourseCategory { get; set; }

    public virtual ICollection<ApplyCourse> ApplyCourses { get; set; } = new List<ApplyCourse>();
}
