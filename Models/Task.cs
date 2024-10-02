namespace ASP.NET_Project.Models;

public class Task 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public int ProjectId { get; set; }
    public int AssignedToId { get; set; }

    public virtual Project Project { get; set; }
    public virtual User AssignedTo { get; set; }

    public virtual ICollection<Progress> Progresses { get; set; } = new List<Progress>();


}

public enum TaskStatus 
{
    InProgress,
    Completed,
    Overdue
}