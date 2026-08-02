// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           CreateEngagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateEngagementCommand. Pure orchestration: constructs the
//                  Engagement aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.Engagement.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Engagement.Commands;

/// <summary>Handles <see cref="CreateEngagementCommand"/>.</summary>
public sealed class CreateEngagementCommandHandler : IRequestHandler<CreateEngagementCommand, Result<CreateEngagementResult>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<CreateEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateEngagementCommandHandler(IEngagementRepository repository, ILogger<CreateEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateEngagementResult>> Handle(CreateEngagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var workerId = UserId.FromGuid(request.WorkerId);

            var aggregate = PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                workerId: workerId,
                engagementScore: request.EngagementScore,
                surveyPeriod: request.SurveyPeriod,
                participationChannel: request.ParticipationChannel,
                responseCount: request.ResponseCount,
                lastEngagedAtUtc: request.LastEngagedAtUtc.HasValue ? UtcTimestamp.From(request.LastEngagedAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateEngagementResult>.Success(new CreateEngagementResult(
                aggregate.EngagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Engagement for Tenant {TenantId}", request.TenantId);

            return Result<CreateEngagementResult>.Failure(new ApplicationError(
                "Analytics.Engagement.CreateFailed",
                "Unable to submit the engagement measurement at this time.",
                Guid.NewGuid()));
        }
    }
}
