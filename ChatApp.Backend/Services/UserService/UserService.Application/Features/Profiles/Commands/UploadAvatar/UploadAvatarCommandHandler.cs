using ChatApp.Shared.Events;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Profiles.Commands.UploadAvatar
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
    {
        private readonly IRepository<UserProfile> _userProfileRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public UploadAvatarCommandHandler(
            IRepository<UserProfile> userProfileRepository,
            IBlobStorageService blobStorageService,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint
        )
        {
            _userProfileRepository = userProfileRepository;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var profile = await _userProfileRepository.GetAsync<UserProfile>(
                predicate: p => p.Id == request.UserId
            );
            if (profile == null)
                throw new NotFoundException(nameof(UserProfile), request.UserId);

            var containerName = _configuration["AzureStorage:AvatarContainer"] ?? "avatars";
            if (!string.IsNullOrEmpty(profile.AvatarUrl))
            {
                // Xóa avatar cũ nếu có
                await _blobStorageService.DeleteFileAsync(profile.AvatarUrl, containerName);
            }

            var newAvatarUrl = await _blobStorageService.UploadFileAsync(
                fileStream: request.FileStream,
                fileName: $"{request.UserId}_{Guid.NewGuid()}_{request.FileName}",
                contentType: request.ContentType,
                containerName: containerName
            );

            profile.UpdateProfile(profile.FullName, newAvatarUrl, profile.Bio);
            await _userProfileRepository.SaveChangesAsync();

            // Publish event để các service khác cập nhật thông tin avatar mới
            var userUpdatedEvent = new UserUpdatedEvent
            {
                UserId = profile.Id,
                FullName = profile.FullName ?? "",
                AvatarUrl = newAvatarUrl
            };
            await _publishEndpoint.Publish(userUpdatedEvent, cancellationToken);

            return newAvatarUrl;
        }
    }
}