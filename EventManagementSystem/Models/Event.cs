using System;

namespace EventManagementSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Date { get; set; } // Ensure this is a DateTime or DateTimeOffset type
        public string Location { get; set; }
        public int MaxParticipants { get; set; }
        public int RegisteredParticipants { get; set; }
    }
}
