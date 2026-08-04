// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCommunityContactsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityContact.Queries;
using PlantSafety.Application.CommunitySafety.CommunityContact.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityContact.Handlers;

/// <summary>Handles <see cref="GetCommunityContactsQuery"/>.</summary>
public sealed class GetCommunityContactsQueryHandler : IRequestHandler<GetCommunityContactsQuery, Result<CommunityContactsQueryResult>>
{
    private readonly ICommunityContactRepository _repository;
    private readonly ILogger<GetCommunityContactsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCommunityContactsQueryHandler(ICommunityContactRepository repository, ILogger<GetCommunityContactsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityContactsQueryResult>> Handle(GetCommunityContactsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCommunityContactItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CommunityContactsQueryResult>.Success(new CommunityContactsQueryResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CommunityContact records for Tenant {TenantId}", request.TenantId);

            return Result<CommunityContactsQueryResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityContact.RetrievalFailed",
                "Unable to retrieve CommunityContact records at this time.",
                Guid.NewGuid()));
        }
    }
}
