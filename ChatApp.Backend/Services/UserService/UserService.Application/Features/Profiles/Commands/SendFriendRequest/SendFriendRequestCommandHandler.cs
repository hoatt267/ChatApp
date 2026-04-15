using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Application.Features.Profiles.Commands.SendFriendRequest
{
    public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, FriendshipDto>
    {
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<Friendship> _friendshipRepository;
        private readonly IMapper _mapper;

        public SendFriendRequestCommandHandler(
            IRepository<UserProfile> profileRepository,
            IRepository<Friendship> friendshipRepository,
            IMapper mapper
            )
        {
            _profileRepository = profileRepository;
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task<FriendshipDto> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
        {
            // 1. Không được tự kết bạn với chính mình
            if (request.RequesterId == request.ReceiverId)
                throw new BadRequestException("You cannot send a friend request to yourself.");

            // 2. Kiểm tra người nhận có tồn tại không
            var receiverExists = await _profileRepository.ExistsAsync(u => u.Id == request.ReceiverId);
            if (!receiverExists) throw new NotFoundException(nameof(UserProfile), request.ReceiverId);

            // 3. Kiểm tra xem giữa 2 người ĐÃ CÓ mối quan hệ nào chưa (A->B hoặc B->A)
            var existingFriendship = await _friendshipRepository.GetAsync<Friendship>(
                predicate: f => (f.RequesterId == request.RequesterId && f.ReceiverId == request.ReceiverId) ||
                                (f.RequesterId == request.ReceiverId && f.ReceiverId == request.RequesterId)
            );

            if (existingFriendship != null)
            {
                if (existingFriendship.Status == FriendshipStatus.Accepted)
                    throw new BadRequestException("You are already friends.");

                if (existingFriendship.Status == FriendshipStatus.Pending)
                {
                    // Nếu B đã gửi cho A trước đó, mà giờ A lại gửi cho B -> Tự động thành bạn bè luôn!
                    if (existingFriendship.ReceiverId == request.RequesterId)
                    {
                        existingFriendship.Accept();
                        await _friendshipRepository.SaveChangesAsync();
                        return _mapper.Map<FriendshipDto>(existingFriendship);
                    }
                    throw new BadRequestException("Friend request is already pending.");
                }

                if (existingFriendship.Status == FriendshipStatus.Blocked)
                    throw new BadRequestException("Cannot send friend request.");
            }

            // 4. Nếu chưa có gì, tạo mới request Pending
            var friendship = new Friendship(request.RequesterId, request.ReceiverId);
            await _friendshipRepository.AddAsync(friendship);

            return _mapper.Map<FriendshipDto>(friendship);
        }
    }
}