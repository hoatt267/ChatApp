using AutoMapper;
using ChatApp.Shared.Exceptions;
using ChatApp.Shared.Interfaces;
using MediatR;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;

namespace UserService.Application.Features.Profiles.Queries.GetProfile
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
    {
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IMapper _mapper;

        public GetProfileQueryHandler(IRepository<UserProfile> profileRepository, IMapper mapper)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
        }

        public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetAsync<UserProfile>(predicate: p => p.Id == request.UserId)
                ?? throw new NotFoundException(nameof(UserProfile), request.UserId);

            return _mapper.Map<ProfileDto>(profile);
        }
    }
}