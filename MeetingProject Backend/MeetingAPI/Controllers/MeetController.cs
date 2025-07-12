using Entities.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;
using Services.Interface;

[ApiController]
[Route("api/[controller]")]
public class MeetController : ControllerBase
{
    private readonly IMeetingService _meetingService;
    private readonly ITokenService _tokenService;
    private readonly IHubContext<ChatHub> _hubContext;

    public MeetController(IMeetingService meetingService, ITokenService tokenService, IHubContext<ChatHub> hubContext)
    {
        _meetingService = meetingService;
        _tokenService = tokenService;
        _hubContext = hubContext;
    }

    [HttpPost("Create")]
    public IActionResult CreateMeeting([FromBody] CreateMeetingRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { Message = "Title is required." });

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized(new { Message = "Token bulunamadı." });

            var userId = _tokenService.ValidateTokenAndGetUserId(token);
            if (userId == null)
                return Unauthorized(new { Message = "Geçersiz token." });

            var meeting = _meetingService.CreateMeeting(userId: userId.Value, title: request.Title, participantIds: request.ParticipantIds);

            // Katılımcıları gruba otomatik ekle ve bildirim gönder
            if (request.ParticipantIds != null && request.ParticipantIds.Any())
            {
                foreach (var participantId in request.ParticipantIds)
                {
                    _hubContext.Groups.AddToGroupAsync(participantId.ToString(), meeting.MeetId.ToString());
                    _hubContext.Clients.Group(participantId.ToString()).SendAsync("MeetingCreated", new
                    {
                        MeetingId = meeting.MeetId,
                        Title = meeting.Title,
                        CreatedAt = meeting.CreatedAt
                    });
                }
            }

            return Ok(new
            {
                MeetingId = meeting.MeetId,
                Title = meeting.Title,
                CreatedAt = meeting.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while creating the meeting.", Error = ex.Message });
        }
    }

    [HttpPost("{meetingId}/AddParticipants")]
    public IActionResult JoinMeeting(string meetingId)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized(new { Message = "Token bulunamadı." });

            var userId = _tokenService.ValidateTokenAndGetUserId(token);
            if (userId == null)
                return Unauthorized(new { Message = "Geçersiz token." });

            _meetingService.AddParticipantToMeeting(meetingId, userId.Value);

            return Ok(new { Message = "User successfully joined the meeting." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while joining the meeting.", Error = ex.Message });
        }
    }


    [HttpPost("{meetingId}/SendMessage")]
    public IActionResult SendMessage(string meetingId, [FromBody] MessageRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Message) || string.IsNullOrEmpty(request.User))
                return BadRequest(new { Message = "Message and User are required." });

            _hubContext.Clients.Group(meetingId).SendAsync("ReceiveMessage", request.User, request.Message);

            return Ok(new { Message = "Message sent successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while sending the message.", Error = ex.Message });
        }
    }
}
