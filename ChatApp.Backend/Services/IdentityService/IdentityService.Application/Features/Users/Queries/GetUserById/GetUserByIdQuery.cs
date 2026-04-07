using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.DTOs;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<UserResponseDto>;
}