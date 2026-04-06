using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Application.DTOs;
using MediatR;

namespace ChatService.Application.Features.Chats.Commands.UploadMessageMedia
{
    public record UploadMessageMediaCommand(
        Guid ConversationId,
        Guid SenderId,
        Stream FileStream,
        string FileName,
        string ContentType
    ) : IRequest<MessageDto>;
}