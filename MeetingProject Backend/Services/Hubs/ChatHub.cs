using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Services.Hubs
{
    public class ChatHub : Hub
    {
        // ConnectionId → MeetingId eşlemesi
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Toplantıya katılma
        public async Task JoinMeeting(string meetingId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
                _connections[Context.ConnectionId] = meetingId;

                _logger.LogInformation($"[JoinMeeting] {Context.ConnectionId} joined meeting {meetingId}");

                await Clients.Caller.SendAsync("JoinMeetingSuccess", meetingId);
                // Opsiyonel: diğer katılımcılara bildir
                await Clients.OthersInGroup(meetingId).SendAsync("UserJoined", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[JoinMeeting] Error joining meeting {meetingId} for {Context.ConnectionId}");
                throw;
            }
        }

        // Toplantıdan ayrılma
        public async Task LeaveMeeting(string meetingId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
                _connections.TryRemove(Context.ConnectionId, out _);

                _logger.LogInformation($"[LeaveMeeting] {Context.ConnectionId} left meeting {meetingId}");
                await Clients.Caller.SendAsync("LeaveMeetingSuccess", meetingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[LeaveMeeting] Error for {Context.ConnectionId} in meeting {meetingId}");
                throw;
            }
        }

        // WebRTC sinyalleşme (offer, answer, iceCandidate)
        public async Task SendSignal(string meetingId, string senderId, string signalData, string targetConnectionId)
        {
            try
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", senderId, signalData);
                _logger.LogInformation($"[SendSignal] From {senderId} to {targetConnectionId} in {meetingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SendSignal] Failed from {senderId} to {targetConnectionId} in {meetingId}");
                throw;
            }
        }

        // Metin mesaj gönderme
        public async Task SendMessage(string meetingId, string senderId, string message)
        {
            try
            {
                _logger.LogInformation($"[SendMessage] {senderId} → {meetingId}: {message}");
                await Clients.Group(meetingId).SendAsync("ReceiveMessage", senderId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SendMessage] Error sending message in {meetingId} from {senderId}");
                throw;
            }
        }

        // Yeni katılımcıyı diğerlerine bildir
        public async Task NotifyNewParticipant(string meetingId, string newConnectionId)
        {
            try
            {
                await Clients.GroupExcept(meetingId, newConnectionId).SendAsync("NewParticipantJoined", newConnectionId);
                _logger.LogInformation($"[NotifyNewParticipant] Notified group {meetingId} about {newConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[NotifyNewParticipant] Error notifying group {meetingId}");
                throw;
            }
        }

        // Kullanıcı bağlantısı koparsa
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var meetingId))
            {
                _logger.LogInformation($"[Disconnected] {Context.ConnectionId} disconnected from meeting {meetingId}");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
            }
            else
            {
                _logger.LogWarning($"[Disconnected] {Context.ConnectionId} was not found in active connections.");
            }

            if (exception != null)
            {
                _logger.LogError(exception, $"[Disconnected] Exception for {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
