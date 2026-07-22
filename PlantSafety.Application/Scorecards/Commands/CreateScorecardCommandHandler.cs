// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- realigned with the real Scorecard.Create() reporting-period signature.
// FILE DESCRIPTION: Handler for CreateScorecardCommand. Pure orchestration: constructs the Scorecard aggregate via its domain factory method and persists it through the domain repository interface. No business rules live here.
// SchemaVersion: 2

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scorecards.Commands;

/// <summary>Handles <see cref="CreateScorecardCommand"/>.</summary>
public sealed class CreateScorecardCommandHandler : IRequestHandler<CreateScorecardCommand, Result<CreateScorecardResult>>
{
    private readonly IScorecardRepository _repository;
    private readonly ILogger<CreateScorecardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateScorecardCommandHandler(IScorecardRepository repository, ILogger<CreateScorecardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateScorecardResult>> Handle(CreateScorecardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = UserId.FromGuid(request.OwnerId);
            var category = IndicatorCategory.IsValid(request.Category) ? IndicatorCategory.From(request.Category) : IndicatorCategory.Default;
            var periodType = ScorecardPeriodType.IsValid(request.PeriodType) ? ScorecardPeriodType.From(request.PeriodType) : ScorecardPeriodType.Default;

            var aggregate = PlantSafety.Domain.Scorecards.Aggregates.Scorecard.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(ownerId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                scorecardTitle: request.ScorecardTitle,
                category: category,
                periodType: periodType,
                periodStartUtc: UtcTimestamp.From(request.PeriodStartUtc),
                ownerId: ownerId,
                reviewRequired: request.ReviewRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateScorecardResult>.Success(new CreateScorecardResult(
                aggregate.ScorecardId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Scorecard period for Tenant {TenantId}", request.TenantId);

            return Result<CreateScorecardResult>.Failure(new ApplicationError(
                "Scorecards.Scorecard.CreateFailed",
                "Unable to create the scorecard record at this time.",
                Guid.NewGuid()));
        }
    }
}
