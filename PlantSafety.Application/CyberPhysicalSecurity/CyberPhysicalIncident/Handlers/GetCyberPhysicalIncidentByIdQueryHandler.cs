// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCyberPhysicalIncidentByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Handlers;

/// <summary>Handles <see cref="GetCyberPhysicalIncidentByIdQuery"/>.</summary>
public sealed class GetCyberPhysicalIncidentByIdQueryHandler : IRequestHandler<GetCyberPhysicalIncidentByIdQuery, Result<CyberPhysicalIncidentDto>>
{
    private readonly ICyberPhysicalIncidentReadRepository _repository;
    private readonly ILogger<GetCyberPhysicalIncidentByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCyberPhysicalIncidentByIdQueryHandler(ICyberPhysicalIncidentReadRepository repository, ILogger<GetCyberPhysicalIncidentByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CyberPhysicalIncidentDto>> Handle(GetCyberPhysicalIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.CyberPhysicalIncidentId, request.TenantId, cancellationToken);

            if (item is null)
            {
                _logger.LogDebug("CyberPhysicalIncident {CyberPhysicalIncidentId} not found for Tenant {TenantId}", request.CyberPhysicalIncidentId, request.TenantId);

                return Result<CyberPhysicalIncidentDto>.Failure(new ApplicationError(
                    "CyberPhysicalSecurity.CyberPhysicalIncident.NotFound",
                    "The requested cyber-physical incident could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CyberPhysicalIncidentDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CyberPhysicalIncident {CyberPhysicalIncidentId}", request.CyberPhysicalIncidentId);

            return Result<CyberPhysicalIncidentDto>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.CyberPhysicalIncident.RetrievalFailed",
                "Unable to retrieve cyber-physical incident data at this time.",
                Guid.NewGuid()));
        }
    }
}
