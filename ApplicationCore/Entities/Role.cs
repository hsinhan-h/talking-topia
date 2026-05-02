using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int RoleType { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
