using ChatService.Domain.Enums;
using FluentValidation;

namespace ChatService.Application.Features.Chats.Commands
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Message content cannot be empty.")
                .When(x => x.Type == MessageType.Text);

            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage("File URL cannot be empty.")
                .When(x => x.Type != MessageType.Text);

            // Kiểm tra ID phòng chat phải hợp lệ (không phải Guid rỗng)
            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage("Invalid conversation ID.");
            // Kiểm tra ID người gửi
            RuleFor(x => x.SenderId)
                .NotEmpty().WithMessage("Invalid sender ID.");
        }
    }
}