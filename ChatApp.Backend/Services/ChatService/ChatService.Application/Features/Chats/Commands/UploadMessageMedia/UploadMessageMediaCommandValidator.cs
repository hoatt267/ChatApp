using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace ChatService.Application.Features.Chats.Commands.UploadMessageMedia
{
    public class UploadMessageMediaCommandValidator : AbstractValidator<UploadMessageMediaCommand>
    {
        public UploadMessageMediaCommandValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage("Invalid conversation ID.");

            RuleFor(x => x.SenderId)
                .NotEmpty().WithMessage("Invalid sender ID.");

            RuleFor(x => x.FileStream)
                .NotNull().WithMessage("File stream cannot be null.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("Content type is required.");
        }
    }
}