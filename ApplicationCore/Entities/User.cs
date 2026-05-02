using System;
using System.Collections.Generic;


namespace ApplicationCore.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string LineId { get; set; }

    public string PictureUrl { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
