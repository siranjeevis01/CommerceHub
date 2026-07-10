using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;
