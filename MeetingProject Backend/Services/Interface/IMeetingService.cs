using Entities.Models;

namespace Services.Interface
{
    public interface IMeetingService
    {
        Meeting CreateMeeting(int userId, string title, List<int>? participantIds);
        bool AddParticipantToMeeting(string meetingId, int userId);
    }
}
