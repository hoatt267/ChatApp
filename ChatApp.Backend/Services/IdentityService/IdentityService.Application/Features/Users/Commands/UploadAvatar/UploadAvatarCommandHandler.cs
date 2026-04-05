using ChatApp.Shared.Events;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Application.Features.Users.Commands.UploadAvatar
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public UploadAvatarCommandHandler(
            IRepository<User> userRepository,
            IBlobStorageService blobStorageService,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync<User>(predicate: u => u.Id == request.UserId);
            if (user == null) throw new NotFoundException(nameof(User), request.UserId);

            // Lấy tên "Xô" (Container) từ appsettings.json
            var containerName = _configuration["AzureStorage:AvatarContainer"] ?? "avatars";

            // 1. Tối ưu dung lượng: Nếu có avatar cũ, xóa nó đi!
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _blobStorageService.DeleteFileAsync(user.AvatarUrl, containerName);
            }

            // 2. Upload file mới lên Azurite
            var newAvatarUrl = await _blobStorageService.UploadFileAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                containerName);

            // 3. Cập nhật URL mới vào DB (giữ nguyên FullName cũ)
            user.UpdateProfile(user.FullName, newAvatarUrl);
            await _userRepository.SaveChangesAsync();

            // 4. Bắn sự kiện cho ChatService biết để đồng bộ
            var userUpdatedEvent = new UserUpdatedEvent
            {
                UserId = user.Id,
                FullName = user.FullName ?? "",
                AvatarUrl = newAvatarUrl
            };
            await _publishEndpoint.Publish(userUpdatedEvent, cancellationToken);

            return newAvatarUrl;
        }
    }
}