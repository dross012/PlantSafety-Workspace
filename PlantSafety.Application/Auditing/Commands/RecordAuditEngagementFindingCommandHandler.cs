// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordAuditEngagementFindingCommand. Pure orchestration: loads the AuditEngagement aggregate, invokes its RecordFinding() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="RecordAuditEngagementFindingCommand"/>.</summary>
public sealed class RecordAuditEngagementFindingCommandHandler : IRequestHandler<RecordAuditEngagementFindingCommand, Result<RecordAuditEngagementFindingResult>>
{
    private readonly IAuditEngagementRepository _repository;
    private readonly ILogger<RecordAuditEngagementFindingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAuditEngagementFindingCommandHandler(IAuditEngagementRepository repository, ILogger<RecordAuditEngagementFindingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordAuditEngagementFindingResult>> Handle(RecordAuditEngagementFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditEngagementId.FromGuid(request.AuditEngagementId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordAuditEngagementFindingResult>.Failure(new ApplicationError(
                    "Auditing.AuditEngagement.NotFound",
                    "The requested AuditEngagement record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.RecordFinding(
                auditFindingId: request.AuditFindingId,
                severity: request.Severity,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RecordAuditEngagementFindingResult>.Success(new RecordAuditEngagementFindingResult(
                aggregate.AuditEngagementId.Value,
                aggregate.FindingsCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record finding on AuditEngagement {AuditEngagementId}", request.AuditEngagementId);

            return Result<RecordAuditEngagementFindingResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.RecordFindingFailed",
                "Unable to record the finding at this time.",
                request.CorrelationId));
        }
    }
}
