using MediatR;
using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Queries.Role;

public record GetAllRolesQuery : IRequest<List<RoleDto>>;
