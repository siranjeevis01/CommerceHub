using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record DeleteBrandCommand(int Id) : IRequest;
