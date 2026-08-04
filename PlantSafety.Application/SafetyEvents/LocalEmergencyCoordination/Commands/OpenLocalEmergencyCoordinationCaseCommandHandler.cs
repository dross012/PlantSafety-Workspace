// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           OpenLocalEmergencyCoordinationCaseCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for OpenLocalEmergencyCoordinationCaseCommand. Pure orchestration:
//                  constructs the aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here. Mirrors
//                  DeclareEmergencyCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Handles <see cref="OpenLocalEmergencyCoordinationCaseCommand"/>.</summary>
public sealed class OpenLocalEmergencyCoordinationCaseCommandHandler : IRequestHandler<OpenLocalEmergencyCoordinationCaseCommand, Result<OpenLocalEmergencyCoordinationCaseResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<OpenLocalEmergencyCoordinationCaseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public OpenLocalEmergencyCoordinationCaseCommandHandler(ILocalEmergencyCoordinationRepository repository, ILogger<OpenLocalEmergencyCoordinationCaseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OpenLocalEmergencyCoordinationCaseResult>> Handle(OpenLocalEmergencyCoordinationCaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var openedBy = UserId.FromGuid(request.OpenedByUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination.Create(
                createdBy: openedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(openedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentId: request.IncidentId,
                caseName: request.CaseName);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<OpenLocalEmergencyCoordinationCaseResult>.Success(new OpenLocalEmergencyCoordinationCaseResult(
                aggregate.LocalEmergencyCoordinationId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open LocalEmergencyCoordination case for Tenant {TenantId}", request.TenantId);

            return Result<OpenLocalEmergencyCoordinationCaseResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.OpenFailed",
                "Unable to open the local emergency coordination case at this time.",
                Guid.NewGuid()));
        }
    }
}
