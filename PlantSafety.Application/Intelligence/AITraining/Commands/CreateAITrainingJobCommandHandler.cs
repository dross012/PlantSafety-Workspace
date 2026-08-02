// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           CreateAITrainingJobCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateAITrainingJobCommand. Pure orchestration: constructs the
//                  AITrainingJob aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AITraining.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AITraining.Commands;

/// <summary>Handles <see cref="CreateAITrainingJobCommand"/>.</summary>
public sealed class CreateAITrainingJobCommandHandler : IRequestHandler<CreateAITrainingJobCommand, Result<AITrainingJobStatusResult>>
{
    private readonly IAITrainingJobRepository _repository;
    private readonly ILogger<CreateAITrainingJobCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAITrainingJobCommandHandler(IAITrainingJobRepository repository, ILogger<CreateAITrainingJobCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AITrainingJobStatusResult>> Handle(CreateAITrainingJobCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var initiatedBy = UserId.FromGuid(request.InitiatedByUserId);

            var aggregate = PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob.Create(
                createdBy: initiatedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(initiatedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                modelName: request.ModelName,
                modelVersion: request.ModelVersion,
                datasetReference: request.DatasetReference,
                initiatedBy: initiatedBy);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AITrainingJobStatusResult>.Success(new AITrainingJobStatusResult(
                aggregate.AITrainingJobId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AITrainingJob for Tenant {TenantId}", request.TenantId);

            return Result<AITrainingJobStatusResult>.Failure(new ApplicationError(
                "Intelligence.AITraining.CreateFailed",
                "Unable to create the AI training job at this time.",
                Guid.NewGuid()));
        }
    }
}
