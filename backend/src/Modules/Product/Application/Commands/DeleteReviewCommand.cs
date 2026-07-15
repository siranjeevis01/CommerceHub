using MediatR;

namespace CommerceHub.Modules.Product.Application.Commands;

public record DeleteReviewCommand(int Id) : IRequest;
