using ChatApp.Shared.Events;
using ChatApp.Shared.Interfaces;
using IdentityService.Domain.Entities;
using MassTransit;
using MediatR;

namespace IdentityService.Application.Features.Users.Commands.SyncOldUsers
{
    public class SyncOldUsersCommandHandler : IRequestHandler<SyncOldUsersCommand, int>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public SyncOldUsersCommandHandler(IRepository<User> userRepository, IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<int> Handle(SyncOldUsersCommand request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetListAsync<User>();
            int syncedCount = 0;

            foreach (var user in users)
            {
                var userCreatedEvent = new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName ?? "No Name"
                };

                await _publishEndpoint.Publish(userCreatedEvent, cancellationToken);
                syncedCount++;
            }

            return syncedCount;
        }
    }
}