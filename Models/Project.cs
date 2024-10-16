    using System.Collections.Generic;

    namespace ASP.NET_Project.Models
    {
        public enum ProjectStatus
        {
            InProgress,
            Completed,
            Overdue
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

    public int? ProjectManagerId { get; set; }
    public virtual User ProjectManager { get; set; }

    public ProjectStatus Status { get; set; }

    // Navigation property for many-to-many relationship with Users
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }

    }

