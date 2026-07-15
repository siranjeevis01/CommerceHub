using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record DeleteBrandCommand(int Id) : IRequest;
