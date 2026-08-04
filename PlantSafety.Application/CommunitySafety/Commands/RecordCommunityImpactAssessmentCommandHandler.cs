// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordCommunityImpactAssessmentCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Aggregates.CommunityImpactAssessment;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="RecordCommunityImpactAssessmentCommand"/>.</summary>
public sealed class RecordCommunityImpactAssessmentCommandHandler : IRequestHandler<RecordCommunityImpactAssessmentCommand, Result<CommunityImpactAssessmentActionResult>>
{
    private readonly ICommunityImpactAssessmentRepository _repository;
    private readonly ILogger<RecordCommunityImpactAssessmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCommunityImpactAssessmentCommandHandler(ICommunityImpactAssessmentRepository repository, ILogger<RecordCommunityImpactAssessmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityImpactAssessmentActionResult>> Handle(RecordCommunityImpactAssessmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<CommunityImpactSeverity>(request.Severity, ignoreCase: true, out var severity))
            {
                return Result<CommunityImpactAssessmentActionResult>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityImpactAssessment.InvalidSeverity",
                    $"'{request.Severity}' is not a recognized CommunityImpactAssessment severity.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var assessedBy = UserId.FromGuid(request.AssessedByUserId);

            var aggregate = Agg.Record(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentId: request.IncidentId,
                severity: severity,
                affectedArea: request.AffectedArea,
                description: request.Description,
                assessedByUserId: assessedBy,
                estimatedPersonsAffected: request.EstimatedPersonsAffected);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunityImpactAssessmentActionResult>.Success(new CommunityImpactAssessmentActionResult(
                aggregate.CommunityImpactAssessmentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record CommunityImpactAssessment for Incident {IncidentId}", request.IncidentId);

            return Result<CommunityImpactAssessmentActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityImpactAssessment.RecordFailed",
                "Unable to record the CommunityImpactAssessment at this time.",
                Guid.NewGuid()));
        }
    }
}
