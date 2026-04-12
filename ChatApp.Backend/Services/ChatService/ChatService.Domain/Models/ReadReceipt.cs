namespace ChatService.Domain.Models
{
    public class ReadReceipt
    {
        public Guid UserId { get; set; }
        public DateTime ReadAt { get; set; }
    }
}