namespace ASP.NET_Project.Models;
public class Project 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate   { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
    public int CreatedById { get; set; }

    public virtual User CreatedBy { get; set; }
    public virtual ICollection<Task> Tasks  { get; set; }
}