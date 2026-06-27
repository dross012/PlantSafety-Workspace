// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRcaByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.RootCauseAnalysis;
using PlantSafety.Application.Features.SafetyEvents.Queries.RootCauseAnalysis.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;

/// <summary>Handles <see cref="GetRcaByIdQuery"/>.</summary>
public sealed class GetRcaByIdQueryHandler : IQueryHandler<GetRcaByIdQuery, RootCauseAnalysisDto?>
{
    private readonly IRcaRepository _repository;
    private readonly ILogger<GetRcaByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRcaByIdQueryHandler(IRcaRepository repository, ILogger<GetRcaByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RootCauseAnalysisDto?> Handle(GetRcaByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.RcaId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("RCA {RcaId} not found for Tenant {TenantId}", request.RcaId, request.TenantId);
            return null;
        }

        return new RootCauseAnalysisDto
        {
            RcaId = model.RcaId,
            TenantId = model.TenantId,
            LinkedEventId = model.LinkedEventId,
            LinkedEventType = model.LinkedEventType,
            Methodology = model.Methodology,
            RootCauses = model.RootCauses,
            CorrectiveActions = model.CorrectiveActions,
            Status = model.Status,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
