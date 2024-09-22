namespace ASP.NET_Project.Models
{
    public enum ProjectStatus
    {
        Pending,
        InProgress,
        Completed
    }

   public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now; 
    public DateTime EndDate { get; set; } // Due date
    public bool IsActive { get; set; } = true;

    public int CreatedById { get; set; }
    public virtual User CreatedBy { get; set; }

    public int? ProjectManagerId { get; set; } // Foreign key for project manager
    public virtual User ProjectManager { get; set; } // Navigation property for project manager

    public ProjectStatus Status { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
}
