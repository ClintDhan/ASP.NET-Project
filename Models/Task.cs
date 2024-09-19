namespace ASP.NET_Project.Models;

public class Task 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    
    public int ProjectId { get; set; }
    public int AssignedToId { get; set; }

    public virtual Project Project { get; set; }
    public virtual User AssignedTo { get; set; }

}