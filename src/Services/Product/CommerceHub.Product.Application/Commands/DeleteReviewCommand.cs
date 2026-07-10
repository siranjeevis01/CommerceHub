using MediatR;

namespace CommerceHub.Product.Application.Commands;

public record DeleteReviewCommand(int Id) : IRequest;
