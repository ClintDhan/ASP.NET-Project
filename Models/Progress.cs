namespace ASP.NET_Project.Models
{
    public class Progress 
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public virtual Task Task { get; set; }
        public virtual User User { get; set; }
        
    }
}