using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;
