// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for StartAuditEngagementCommand. Pure orchestration: loads the AuditEngagement aggregate, invokes its Start() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;
using PlantSafety.Domain.Auditing.AuditEngagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="StartAuditEngagementCommand"/>.</summary>
public sealed class StartAuditEngagementCommandHandler : IRequestHandler<StartAuditEngagementCommand, Result<StartAuditEngagementResult>>
{
    private readonly IAuditEngagementRepository _repository;
    private readonly ILogger<StartAuditEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartAuditEngagementCommandHandler(IAuditEngagementRepository repository, ILogger<StartAuditEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StartAuditEngagementResult>> Handle(StartAuditEngagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditEngagementId.FromGuid(request.AuditEngagementId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<StartAuditEngagementResult>.Failure(new ApplicationError(
                    "Auditing.AuditEngagement.NotFound",
                    "The requested AuditEngagement record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Start(
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<StartAuditEngagementResult>.Success(new StartAuditEngagementResult(
                aggregate.AuditEngagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start AuditEngagement {AuditEngagementId}", request.AuditEngagementId);

            return Result<StartAuditEngagementResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.StartFailed",
                "Unable to start the AuditEngagement at this time.",
                request.CorrelationId));
        }
    }
}
