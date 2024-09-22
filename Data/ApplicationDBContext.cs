using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Project.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Progress> Progresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "User" },
                new Role { RoleId = 3, RoleName = "ProjectManager" } // Example additional role
            );

            // Configure relationships
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany() // A User can create multiple Projects
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Define behavior on delete

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectManager)
                .WithMany() // A User can manage multiple Projects
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict); // Define behavior on delete

            modelBuilder.Entity<User>()
                .HasOne(u => u.Project)
                .WithMany(p => p.Users) // A Project can have multiple Users
                .HasForeignKey(u => u.ProjectId)
                .OnDelete(DeleteBehavior.Restrict); // Define behavior on delete
        }
    }
}
