// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetGovernedModelByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.AiGovernance.Models;
using PlantSafety.Application.Features.AiGovernance.Queries.GetModel;

namespace PlantSafety.Application.Features.AiGovernance.Handlers;

/// <summary>Handles GetGovernedModelByIdQuery.</summary>
public sealed class GetGovernedModelByIdQueryHandler : IQueryHandler<GetGovernedModelByIdQuery, GovernedModelDto?>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly ILogger<GetGovernedModelByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetGovernedModelByIdQueryHandler(IAiGovernanceRepository repository, ILogger<GetGovernedModelByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<GovernedModelDto?> Handle(GetGovernedModelByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.ModelId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Governed model {ModelId} not found for Tenant {TenantId}", request.ModelId, request.TenantId);
            return null;
        }

        return new GovernedModelDto
        {
            ModelId = model.ModelId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            ModelName = model.ModelName,
            ModelVersion = model.ModelVersion,
            GovernanceStatus = model.GovernanceStatus,
            RiskTier = model.RiskTier,
            DriftStatus = model.DriftStatus,
            LastReviewedAtUtc = model.LastReviewedAtUtc,
            ReviewerName = model.ReviewerName,
        };
    }
}
