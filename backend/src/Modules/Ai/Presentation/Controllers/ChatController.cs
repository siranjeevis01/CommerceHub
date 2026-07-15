using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Ai.Application.Commands.Chat;
using CommerceHub.Modules.Ai.Application.DTOs;

namespace CommerceHub.Modules.Ai.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
    {
        var userId = GetUserId();
        var command = new SendMessageCommand
        {
            UserId = userId,
            Message = request.Message,
            ConversationId = request.ConversationId,
            Context = request.Context
        };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetUserId();
        var command = new CreateConversationCommand
        {
            UserId = userId,
            Title = request.Title,
            InitialMessage = request.InitialMessage
        };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var query = new GetUserConversationsQuery { UserId = userId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetConversationHistory(int id)
    {
        var userId = GetUserId();
        var query = new GetConversationHistoryQuery { ConversationId = id, UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("search")]
    public async Task<IActionResult> NaturalLanguageSearch([FromBody] SearchRequestDto request)
    {
        var userId = GetUserId();
        var command = new NaturalLanguageSearchCommand
        {
            UserId = userId,
            Query = request.Query,
            Page = request.Page,
            PageSize = request.PageSize
        };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromQuery] int count = 10, [FromQuery] string? type = null)
    {
        var userId = GetUserId();
        var query = new GetRecommendationsQuery { UserId = userId, Count = count, Type = type };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("recommendations/trending")]
    public async Task<IActionResult> GetTrending([FromQuery] int count = 10)
    {
        var query = new GetRecommendationsQuery { UserId = 0, Count = count, Type = "trending" };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("interactions")]
    public async Task<IActionResult> RecordInteraction([FromBody] RecordInteractionRequest request)
    {
        var userId = GetUserId();
        var command = new RecordInteractionCommand
        {
            UserId = userId,
            ProductId = request.ProductId,
            InteractionType = request.InteractionType
        };
        await _mediator.Send(command);
        return Ok(new { Success = true });
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { Service = "CommerceHub AI Agent Service", Status = "Running" });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}

public record CreateConversationRequest
{
    public string Title { get; init; } = string.Empty;
    public string? InitialMessage { get; init; }
}

public record SearchRequestDto
{
    public string Query { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record RecordInteractionRequest
{
    public int ProductId { get; init; }
    public string InteractionType { get; init; } = string.Empty;
}
