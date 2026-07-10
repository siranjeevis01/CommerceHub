using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record DeleteProductCommand(int Id) : IRequest;
