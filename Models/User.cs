
namespace ASP.NET_Project.Models;


public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public int? ProjectId { get; set; } // Nullable foreign key
    public virtual Project Project { get; set; }
    public virtual Role Role { get; set; }
}
