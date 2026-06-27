// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR handler for GetIncidentByIdQuery.
//   Reads from the read-side repository (optimised stored proc; no joins to write tables).
//   Returns null when not found rather than throwing — callers may render 404 at the API layer.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="GetIncidentByIdQuery"/>.
/// Returns the full <see cref="IncidentDto"/> or <c>null</c> if not found.
/// </summary>
public sealed class GetIncidentByIdQueryHandler : IQueryHandler<GetIncidentByIdQuery, IncidentDto?>
{
    private readonly IIncidentReadRepository _readRepository;
    private readonly IncidentToIncidentDtoMappingProfile _mapper;
    private readonly ILogger<GetIncidentByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentByIdQueryHandler(
        IIncidentReadRepository readRepository,
        IncidentToIncidentDtoMappingProfile mapper,
        ILogger<GetIncidentByIdQueryHandler> logger)
    {
        _readRepository = readRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IncidentDto?> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        var readModel = await _readRepository.GetByIdAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            cancellationToken: cancellationToken);

        if (readModel is null)
        {
            _logger.LogDebug(
                "Incident {IncidentId} not found for Tenant {TenantId} CorrelationId {CorrelationId}",
                request.IncidentId, request.TenantId, request.CorrelationId);
            return null;
        }

        return _mapper.Map(readModel);
    }
}
