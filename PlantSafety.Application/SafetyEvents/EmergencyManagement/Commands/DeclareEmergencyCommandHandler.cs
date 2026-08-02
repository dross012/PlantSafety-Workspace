// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           DeclareEmergencyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for DeclareEmergencyCommand. Pure orchestration: constructs the
//                  EmergencyManagement aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;

/// <summary>Handles <see cref="DeclareEmergencyCommand"/>.</summary>
public sealed class DeclareEmergencyCommandHandler : IRequestHandler<DeclareEmergencyCommand, Result<DeclareEmergencyResult>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<DeclareEmergencyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeclareEmergencyCommandHandler(IEmergencyManagementRepository repository, ILogger<DeclareEmergencyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DeclareEmergencyResult>> Handle(DeclareEmergencyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var declaredBy = UserId.FromGuid(request.DeclaredByUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement.Create(
                createdBy: declaredBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(declaredBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                emergencyType: request.EmergencyType,
                severity: request.Severity,
                location: request.Location,
                casualtyCount: request.CasualtyCount,
                responseTeamNotified: request.ResponseTeamNotified);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DeclareEmergencyResult>.Success(new DeclareEmergencyResult(
                aggregate.EmergencyManagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare EmergencyManagement for Tenant {TenantId}", request.TenantId);

            return Result<DeclareEmergencyResult>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.DeclareFailed",
                "Unable to declare the emergency at this time.",
                Guid.NewGuid()));
        }
    }
}
