// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           CreatePredictiveSafetyAlertCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreatePredictiveSafetyAlertCommand. Pure orchestration: constructs
//                  the PredictiveSafetyAlert aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Commands;

/// <summary>Handles <see cref="CreatePredictiveSafetyAlertCommand"/>.</summary>
public sealed class CreatePredictiveSafetyAlertCommandHandler : IRequestHandler<CreatePredictiveSafetyAlertCommand, Result<PredictiveSafetyAlertStatusResult>>
{
    private readonly IPredictiveSafetyAlertRepository _repository;
    private readonly ILogger<CreatePredictiveSafetyAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreatePredictiveSafetyAlertCommandHandler(IPredictiveSafetyAlertRepository repository, ILogger<CreatePredictiveSafetyAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveSafetyAlertStatusResult>> Handle(CreatePredictiveSafetyAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var generatedBy = UserId.FromGuid(request.GeneratedByUserId);

            var aggregate = PlantSafety.Domain.Intelligence.PredictiveSafety.Aggregates.PredictiveSafetyAlert.Create(
                createdBy: generatedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(generatedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                riskCategory: request.RiskCategory,
                predictedRiskScore: request.PredictedRiskScore,
                modelVersion: request.ModelVersion,
                generatedBy: generatedBy);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PredictiveSafetyAlertStatusResult>.Success(new PredictiveSafetyAlertStatusResult(
                aggregate.PredictiveSafetyAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PredictiveSafetyAlert for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveSafetyAlertStatusResult>.Failure(new ApplicationError(
                "Intelligence.PredictiveSafety.CreateFailed",
                "Unable to create the predictive safety alert at this time.",
                Guid.NewGuid()));
        }
    }
}
