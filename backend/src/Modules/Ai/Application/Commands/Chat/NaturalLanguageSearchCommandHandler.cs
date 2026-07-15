using AutoMapper;
using MediatR;
using CommerceHub.Modules.Ai.Application.Common.Interfaces;
using CommerceHub.Modules.Ai.Application.DTOs;
using CommerceHub.Modules.Ai.Domain.Entities;
using CommerceHub.Modules.Ai.Domain.Events;
using System.Text.Json;

namespace CommerceHub.Modules.Ai.Application.Commands.Chat;

public class NaturalLanguageSearchCommandHandler : IRequestHandler<NaturalLanguageSearchCommand, SearchResultDto>
{
    private readonly IProductSearchService _searchService;
    private readonly ILLMService _llmService;
    private readonly IAIAgentDbContext _context;
    private readonly IMediator _mediator;

    public NaturalLanguageSearchCommandHandler(
        IProductSearchService searchService,
        ILLMService llmService,
        IAIAgentDbContext context,
        IMediator mediator)
    {
        _searchService = searchService;
        _llmService = llmService;
        _context = context;
        _mediator = mediator;
    }

    public async Task<SearchResultDto> Handle(NaturalLanguageSearchCommand request, CancellationToken ct)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var correctedQuery = await _llmService.CorrectQueryAsync(request.Query, ct);
        var searchResult = await _searchService.NaturalLanguageSearchAsync(correctedQuery ?? request.Query, request.Page, request.PageSize, ct);

        searchResult.CorrectedQuery = correctedQuery;
        searchResult.Intent = await _llmService.DetectIntentAsync(request.Query, ct);

        var searchQuery = new SearchQuery
        {
            UserId = request.UserId,
            RawQuery = request.Query,
            ParsedIntent = searchResult.Intent,
            CorrectedQuery = correctedQuery,
            SearchResults = JsonSerializer.Serialize(searchResult.Items.Take(5).ToList()),
            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
            IsSuccessful = true,
            ExecutedAt = DateTime.UtcNow
        };
        _context.SearchQueries.Add(searchQuery);
        await _context.SaveChangesAsync(ct);

        await _mediator.Publish(new SearchExecuted
        {
            SearchQueryId = searchQuery.Id,
            UserId = request.UserId,
            RawQuery = request.Query,
            Intent = searchResult.Intent ?? "unknown",
            ResultCount = searchResult.TotalCount,
            ResponseTimeMs = stopwatch.ElapsedMilliseconds
        }, ct);

        return searchResult;
    }
}

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, List<ProductRecommendationDto>>
{
    private readonly IRecommendationService _recommendationService;

    public GetRecommendationsQueryHandler(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task<List<ProductRecommendationDto>> Handle(GetRecommendationsQuery request, CancellationToken ct)
    {
        return request.Type switch
        {
            "trending" => await _recommendationService.GetTrendingProductsAsync(request.Count, ct),
            "similar" => throw new NotImplementedException("Use GetSimilarProducts for this type"),
            _ => await _recommendationService.GetPersonalizedRecommendationsAsync(request.UserId, request.Count, ct)
        };
    }
}

public class RecordInteractionCommandHandler : IRequestHandler<RecordInteractionCommand, bool>
{
    private readonly IRecommendationService _recommendationService;

    public RecordInteractionCommandHandler(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task<bool> Handle(RecordInteractionCommand request, CancellationToken ct)
    {
        await _recommendationService.RecordInteractionAsync(request.UserId, request.ProductId, request.InteractionType, ct);
        return true;
    }
}

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IAIAgentDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateConversationCommandHandler(IAIAgentDbContext context, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken ct)
    {
        var conversation = new Conversation
        {
            UserId = request.UserId,
            Title = request.Title,
            Status = "Active"
        };
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(ct);

        await _mediator.Publish(new ConversationStarted
        {
            ConversationId = conversation.Id,
            UserId = request.UserId,
            Title = conversation.Title,
            StartedAt = conversation.CreatedAt
        }, ct);

        return _mapper.Map<ConversationDto>(conversation);
    }
}
