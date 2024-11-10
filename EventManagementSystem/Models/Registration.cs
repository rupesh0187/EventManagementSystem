namespace EventManagementSystem.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
    }
}
