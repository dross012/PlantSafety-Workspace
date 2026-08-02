// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           CreateNearMissesCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateNearMissesCommand. Pure orchestration: constructs the
//                  NearMisses aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Commands
//                  .RaisePredictiveMaintenanceAlertCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.NearMisses.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Handles <see cref="CreateNearMissesCommand"/>.</summary>
public sealed class CreateNearMissesCommandHandler : IRequestHandler<CreateNearMissesCommand, Result<NearMissesStatusResult>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<CreateNearMissesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateNearMissesCommandHandler(INearMissesRepository repository, ILogger<CreateNearMissesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesStatusResult>> Handle(CreateNearMissesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reportedBy = UserId.FromGuid(request.ReportedByUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.NearMisses.Aggregates.NearMisses.Create(
                createdBy: reportedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(reportedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                category: request.Category,
                potentialSeverity: request.PotentialSeverity,
                reportedAtUtc: UtcTimestamp.From(request.ReportedAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<NearMissesStatusResult>.Success(new NearMissesStatusResult(
                aggregate.NearMissesId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create NearMisses for Tenant {TenantId}", request.TenantId);

            return Result<NearMissesStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.CreateFailed",
                "Unable to report the near miss at this time.",
                Guid.NewGuid()));
        }
    }
}
