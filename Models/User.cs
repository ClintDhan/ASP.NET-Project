using System.Collections.Generic;

namespace ASP.NET_Project.Models
{
    public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property for many-to-many relationship with Projects
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual Role Role { get; set; }
}

}
