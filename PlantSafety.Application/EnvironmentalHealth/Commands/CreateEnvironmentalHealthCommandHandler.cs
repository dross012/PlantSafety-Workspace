// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           CreateEnvironmentalHealthCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateEnvironmentalHealthCommand. Pure orchestration: constructs
//                  the EnvironmentalHealth aggregate via its domain factory method and persists
//                  it through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.Commands;

/// <summary>Handles <see cref="CreateEnvironmentalHealthCommand"/>.</summary>
public sealed class CreateEnvironmentalHealthCommandHandler : IRequestHandler<CreateEnvironmentalHealthCommand, Result<CreateEnvironmentalHealthResult>>
{
    private readonly IEnvironmentalHealthRepository _repository;
    private readonly ILogger<CreateEnvironmentalHealthCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateEnvironmentalHealthCommandHandler(IEnvironmentalHealthRepository repository, ILogger<CreateEnvironmentalHealthCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateEnvironmentalHealthResult>> Handle(CreateEnvironmentalHealthCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                monitoringType: request.MonitoringType,
                measuredValue: request.MeasuredValue,
                unit: request.Unit,
                threshold: request.Threshold,
                thresholdExceeded: request.ThresholdExceeded,
                evaluatedStatus: request.EvaluatedStatus,
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateEnvironmentalHealthResult>.Success(new CreateEnvironmentalHealthResult(
                aggregate.EnvironmentalHealthId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create EnvironmentalHealth for Tenant {TenantId}", request.TenantId);

            return Result<CreateEnvironmentalHealthResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.CreateFailed",
                "Unable to submit the environmental-health record at this time.",
                Guid.NewGuid()));
        }
    }
}
