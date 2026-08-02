// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           CreateRootCauseAnalysisCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateRootCauseAnalysisCommand. Pure orchestration: constructs the
//                  RootCauseAnalysis aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Handles <see cref="CreateRootCauseAnalysisCommand"/>.</summary>
public sealed class CreateRootCauseAnalysisCommandHandler : IRequestHandler<CreateRootCauseAnalysisCommand, Result<RootCauseAnalysisStatusResult>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<CreateRootCauseAnalysisCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRootCauseAnalysisCommandHandler(IRootCauseAnalysisRepository repository, ILogger<CreateRootCauseAnalysisCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisStatusResult>> Handle(CreateRootCauseAnalysisCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var leadAnalyst = UserId.FromGuid(request.LeadAnalystUserId);

            var aggregate = PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis.Create(
                createdBy: leadAnalyst,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(leadAnalyst),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                relatedEventId: request.RelatedEventId,
                methodology: request.Methodology,
                leadAnalyst: leadAnalyst);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RootCauseAnalysisStatusResult>.Success(new RootCauseAnalysisStatusResult(
                aggregate.RootCauseAnalysisId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RootCauseAnalysis for Tenant {TenantId}", request.TenantId);

            return Result<RootCauseAnalysisStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.CreateFailed",
                "Unable to start the root cause analysis at this time.",
                Guid.NewGuid()));
        }
    }
}
