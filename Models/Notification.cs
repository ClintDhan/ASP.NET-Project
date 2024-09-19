namespace ASP.NET_Project.Models
{
    public class Notification
    {
        public int Id {get; set;}
        public string Message {get; set;}
        public bool isRead  {get; set;}
        public DateTime CreatedAt {get; set;}

        public int UserId {get; set;}

        public virtual User User{get; set;}
    }
}

