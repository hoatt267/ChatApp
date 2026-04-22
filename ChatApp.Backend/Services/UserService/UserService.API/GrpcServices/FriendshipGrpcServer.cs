using ChatApp.Shared.Interfaces;
using ChatApp.Shared.Protos;
using Grpc.Core;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.API.GrpcServices
{
    public class FriendshipGrpcServer : FriendshipGrpcService.FriendshipGrpcServiceBase
    {
        private readonly IRepository<Friendship> _friendshipRepository;

        public FriendshipGrpcServer(IRepository<Friendship> friendshipRepository)
        {
            _friendshipRepository = friendshipRepository;
        }

        public override async Task<CheckBlockResponse> CheckBlockStatus(CheckBlockRequest request, ServerCallContext context)
        {
            // Parse Guid
            if (!Guid.TryParse(request.UserId1, out var u1) || !Guid.TryParse(request.UserId2, out var u2))
            {
                return new CheckBlockResponse { IsBlocked = false };
            }

            var friendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => ((f.RequesterId == u1 && f.ReceiverId == u2) ||
                                 (f.RequesterId == u2 && f.ReceiverId == u1)) &&
                                f.Status == FriendshipStatus.Blocked
            );

            return new CheckBlockResponse { IsBlocked = friendship != null };
        }
    }
}