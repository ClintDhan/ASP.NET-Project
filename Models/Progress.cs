using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Project.Models
{
    public class Progress 
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public byte[]? FileData { get; set; }   // Store the file as binary data
    public string? FileName { get; set; }   // Store the original file name
    public string? ContentType { get; set; } // Store the MIME type (e.g., "image/png", "application/pdf")

    public virtual Task Task { get; set; }
    public virtual User User { get; set; }

}

}