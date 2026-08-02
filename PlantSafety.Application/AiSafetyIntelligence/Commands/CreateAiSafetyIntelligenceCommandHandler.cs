// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CreateAiSafetyIntelligenceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateAiSafetyIntelligenceCommand. Pure orchestration: constructs
//                  the AiSafetyIntelligence aggregate via its domain factory method and persists it
//                  through the domain repository interface. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AiSafetyIntelligence.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiSafetyIntelligence.Commands;

/// <summary>Handles <see cref="CreateAiSafetyIntelligenceCommand"/>.</summary>
public sealed class CreateAiSafetyIntelligenceCommandHandler : IRequestHandler<CreateAiSafetyIntelligenceCommand, Result<CreateAiSafetyIntelligenceResult>>
{
    private readonly IAiSafetyIntelligenceRepository _repository;
    private readonly ILogger<CreateAiSafetyIntelligenceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateAiSafetyIntelligenceCommandHandler(IAiSafetyIntelligenceRepository repository, ILogger<CreateAiSafetyIntelligenceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateAiSafetyIntelligenceResult>> Handle(CreateAiSafetyIntelligenceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                insightType: request.InsightType,
                description: request.Description,
                confidenceScore: request.ConfidenceScore,
                sourceModelName: request.SourceModelName);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateAiSafetyIntelligenceResult>.Success(new CreateAiSafetyIntelligenceResult(
                aggregate.AiSafetyIntelligenceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid ConfidenceScore submitted for AiSafetyIntelligence create in Tenant {TenantId}", request.TenantId);

            return Result<CreateAiSafetyIntelligenceResult>.Failure(new ApplicationError(
                "AiSafetyIntelligence.InvalidConfidenceScore",
                "ConfidenceScore must be between 0 and 1.",
                Guid.NewGuid(),
                ErrorSeverity.Validation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create AiSafetyIntelligence for Tenant {TenantId}", request.TenantId);

            return Result<CreateAiSafetyIntelligenceResult>.Failure(new ApplicationError(
                "AiSafetyIntelligence.CreateFailed",
                "Unable to register the AI safety intelligence record at this time.",
                Guid.NewGuid()));
        }
    }
}
