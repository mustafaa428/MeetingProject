namespace Entities.Requests
{
    // 📡 WebRTC Signaling için model
    public class SignalRequest
    {
        public string SenderId { get; set; }
        public List<string> TargetConnectionIds { get; set; }
        public string SignalData { get; set; } // Offer, Answer, ICE Candidate gibi veriler
    }
}
