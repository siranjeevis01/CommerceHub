using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record DeleteProductCommand(int Id) : IRequest;
