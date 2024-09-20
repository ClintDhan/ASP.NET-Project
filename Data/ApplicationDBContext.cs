using Microsoft.EntityFrameworkCore;
namespace ASP.NET_Project.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Define your DbSets (tables)
    // public DbSet<YourModel> YourModels { get; set; }
}
