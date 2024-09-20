using Microsoft.EntityFrameworkCore;
namespace ASP.NET_Project.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    
    {}
    public DbSet<User> Users    { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Progress> Progresses{ get; set; }
    
}
