// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for LogCommunityConcernCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="LogCommunityConcernCommand"/>.</summary>
public sealed class LogCommunityConcernCommandHandler : IRequestHandler<LogCommunityConcernCommand, Result<CommunityConcernActionResult>>
{
    private readonly ICommunityConcernRepository _repository;
    private readonly ILogger<LogCommunityConcernCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public LogCommunityConcernCommandHandler(ICommunityConcernRepository repository, ILogger<LogCommunityConcernCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityConcernActionResult>> Handle(LogCommunityConcernCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<CommunityConcernCategory>(request.Category, ignoreCase: true, out var category))
            {
                return Result<CommunityConcernActionResult>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityConcern.InvalidCategory",
                    $"'{request.Category}' is not a recognized CommunityConcern category.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = Agg.Log(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                category: category,
                severity: request.Severity,
                facilityArea: request.FacilityArea,
                description: request.Description,
                reportedByContactId: request.ReportedByContactId,
                relatedIncidentId: request.RelatedIncidentId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunityConcernActionResult>.Success(new CommunityConcernActionResult(
                aggregate.CommunityConcernId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log CommunityConcern for Tenant {TenantId}", request.TenantId);

            return Result<CommunityConcernActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityConcern.LogFailed",
                "Unable to log the CommunityConcern record at this time.",
                Guid.NewGuid()));
        }
    }
}
