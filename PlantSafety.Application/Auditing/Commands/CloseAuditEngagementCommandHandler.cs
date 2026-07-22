// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for CloseAuditEngagementCommand. Pure orchestration: loads the AuditEngagement aggregate, invokes its Close() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="CloseAuditEngagementCommand"/>.</summary>
public sealed class CloseAuditEngagementCommandHandler : IRequestHandler<CloseAuditEngagementCommand, Result<CloseAuditEngagementResult>>
{
    private readonly IAuditEngagementRepository _repository;
    private readonly ILogger<CloseAuditEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAuditEngagementCommandHandler(IAuditEngagementRepository repository, ILogger<CloseAuditEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CloseAuditEngagementResult>> Handle(CloseAuditEngagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditEngagementId.FromGuid(request.AuditEngagementId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CloseAuditEngagementResult>.Failure(new ApplicationError(
                    "Auditing.AuditEngagement.NotFound",
                    "The requested AuditEngagement record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Close(
                complianceScore: request.ComplianceScore,
                complianceRating: request.ComplianceRating,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CloseAuditEngagementResult>.Success(new CloseAuditEngagementResult(
                aggregate.AuditEngagementId.Value,
                aggregate.Status.ToString(),
                aggregate.ComplianceScore,
                aggregate.ComplianceRating,
                aggregate.FindingsCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close AuditEngagement {AuditEngagementId}", request.AuditEngagementId);

            return Result<CloseAuditEngagementResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.CloseFailed",
                "Unable to close the AuditEngagement at this time.",
                request.CorrelationId));
        }
    }
}
