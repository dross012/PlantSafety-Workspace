// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           CreateScoreCardsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateScoreCardsCommand. Pure orchestration: constructs the
//                  ScoreCards aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.ScoreCards.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.ScoreCards.Commands;

/// <summary>Handles <see cref="CreateScoreCardsCommand"/>.</summary>
public sealed class CreateScoreCardsCommandHandler : IRequestHandler<CreateScoreCardsCommand, Result<CreateScoreCardsResult>>
{
    private readonly IScoreCardsRepository _repository;
    private readonly ILogger<CreateScoreCardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateScoreCardsCommandHandler(IScoreCardsRepository repository, ILogger<CreateScoreCardsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateScoreCardsResult>> Handle(CreateScoreCardsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = UserId.FromGuid(request.CreatedByUserId);

            var aggregate = PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards.Create(
                createdBy: createdBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(createdBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                scoreCardName: request.ScoreCardName,
                reportingPeriod: request.ReportingPeriod,
                kpiCategory: request.KpiCategory,
                performanceIndex: request.PerformanceIndex,
                generatedForSiteId: request.GeneratedForSiteId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateScoreCardsResult>.Success(new CreateScoreCardsResult(
                aggregate.ScoreCardsId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ScoreCards for Tenant {TenantId}", request.TenantId);

            return Result<CreateScoreCardsResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.CreateFailed",
                "Unable to create the scorecard at this time.",
                Guid.NewGuid()));
        }
    }
}
