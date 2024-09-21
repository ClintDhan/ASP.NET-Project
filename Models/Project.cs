namespace ASP.NET_Project.Models
{
    public enum ProjectStatus
    {
        Pending,
        InProgress,
        Completed,
        OnHold,
        Canceled
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public ProjectStatus Status { get; set; } // Using enum for status

        public virtual ICollection<Task> Tasks { get; set; }
    }
}
