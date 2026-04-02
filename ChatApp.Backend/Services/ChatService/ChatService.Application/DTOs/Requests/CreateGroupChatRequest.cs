namespace ChatService.Application.DTOs.Requests
{
    public record CreateGroupChatRequest(string Title, List<Guid> TargetUserIds);
}