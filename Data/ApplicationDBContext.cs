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
        new Role { RoleId = 3, RoleName = "ProjectManager" }
    );

    // Many-to-many relationship between User and Project
    modelBuilder.Entity<User>()
        .HasMany(u => u.Projects)
        .WithMany(p => p.Users)
        .UsingEntity(j => j.ToTable("UserProjects"));

    // Configure CreatedBy and ProjectManager relationships
    modelBuilder.Entity<Project>()
        .HasOne(p => p.CreatedBy)
        .WithMany()
        .HasForeignKey(p => p.CreatedById)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Project>()
        .HasOne(p => p.ProjectManager)
        .WithMany()
        .HasForeignKey(p => p.ProjectManagerId)
        .OnDelete(DeleteBehavior.Restrict);
}

    }
}
