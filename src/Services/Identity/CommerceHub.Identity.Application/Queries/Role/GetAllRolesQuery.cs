using MediatR;
using CommerceHub.Identity.Application.DTOs;

namespace CommerceHub.Identity.Application.Queries.Role;

public record GetAllRolesQuery : IRequest<List<RoleDto>>;
