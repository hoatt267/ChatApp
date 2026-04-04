using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Application.DTOs.Responses
{
    public record ParticipantDto(Guid UserId, string FullName, string AvatarUrl = "");
}