// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListGovernedModelsQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.AiGovernance.Models;
using PlantSafety.Application.Features.AiGovernance.Queries.ListModels;

namespace PlantSafety.Application.Features.AiGovernance.Handlers;

/// <summary>Handles ListGovernedModelsQuery.</summary>
public sealed class ListGovernedModelsQueryHandler : IQueryHandler<ListGovernedModelsQuery, IReadOnlyList<GovernedModelDto>>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly ILogger<ListGovernedModelsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListGovernedModelsQueryHandler(IAiGovernanceRepository repository, ILogger<ListGovernedModelsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GovernedModelDto>> Handle(ListGovernedModelsQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.ListAsync(request.TenantId, request.SiteId, cancellationToken);

        return models.Select(m => new GovernedModelDto
        {
            ModelId = m.ModelId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            ModelName = m.ModelName,
            ModelVersion = m.ModelVersion,
            GovernanceStatus = m.GovernanceStatus,
            RiskTier = m.RiskTier,
            DriftStatus = m.DriftStatus,
            LastReviewedAtUtc = m.LastReviewedAtUtc,
            ReviewerName = m.ReviewerName,
        }).ToList();
    }
}
