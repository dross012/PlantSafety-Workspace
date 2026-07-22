// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetConfinedSpaceEntryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Queries;
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Repositories;

namespace PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Handlers;

/// <summary>Handles <see cref="GetConfinedSpaceEntryQuery"/>.</summary>
public sealed class GetConfinedSpaceEntryQueryHandler : IRequestHandler<GetConfinedSpaceEntryQuery, Result<ConfinedSpaceEntryQueryResult>>
{
    private readonly IConfinedSpaceEntryRepository _repository;
    private readonly ILogger<GetConfinedSpaceEntryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetConfinedSpaceEntryQueryHandler(IConfinedSpaceEntryRepository repository, ILogger<GetConfinedSpaceEntryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ConfinedSpaceEntryQueryResult>> Handle(GetConfinedSpaceEntryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetConfinedSpaceEntryAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ConfinedSpaceEntryQueryResult>.Success(new ConfinedSpaceEntryQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ConfinedSpaceEntry for Tenant {TenantId}", request.TenantId);

            return Result<ConfinedSpaceEntryQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.ConfinedSpaceEntry.RetrievalFailed",
                "Unable to retrieve ConfinedSpaceEntry data at this time.",
                Guid.NewGuid()));
        }
    }
}