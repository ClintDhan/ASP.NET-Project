namespace ASP.NET_Project.Models;
public class Role{
    public int RoleId { get; set; }
    public string RoleName { get; set; }

    public virtual ICollection<User> Users { get; set; }
}