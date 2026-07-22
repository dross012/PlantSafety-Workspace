// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetConfinedSpaceEntryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Handlers;

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
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

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
                "Checklists.ChecklistTemplate.ConfinedSpaceEntry.RetrievalFailed",
                "Unable to retrieve Confined Space Entry Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}