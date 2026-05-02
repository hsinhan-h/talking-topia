using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class ApplyCourseCategory
{
    public int ApplyCourseCategoryId { get; set; }

    public string ApplyCategoryName { get; set; }

    public DateTime Cdate { get; set; }

    public DateTime? Udate { get; set; }

    public virtual ICollection<ApplyCourseSubCategory> ApplyCourseSubCategories { get; set; } = new List<ApplyCourseSubCategory>();

    public virtual ICollection<ApplyCourse> ApplyCourses { get; set; } = new List<ApplyCourse>();
}
