using System.Text.Json.Serialization;

namespace UserService.Domain.Enums
{
    // Đánh dấu để ASP.NET Core tự map chuỗi "cancel" thành Enum.Cancel
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FriendshipAction
    {
        Cancel,
        Reject,
        Unfriend
    }
}