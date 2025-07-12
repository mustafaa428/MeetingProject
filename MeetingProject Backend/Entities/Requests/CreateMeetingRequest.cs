namespace Entities.Requests
{
    public class CreateMeetingRequest
    {
        public int UserId { get; set; } // Zorunlu
        public string Title { get; set; } // Zorunlu
        public List<int>? ParticipantIds { get; set; } // Opsiyonel
    }
}
