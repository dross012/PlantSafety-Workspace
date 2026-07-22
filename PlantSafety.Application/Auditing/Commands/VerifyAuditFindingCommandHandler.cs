// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for VerifyAuditFindingCommand. Pure orchestration: loads the AuditFinding aggregate, invokes its Verify() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditFinding.Interfaces;
using PlantSafety.Domain.Auditing.AuditFinding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="VerifyAuditFindingCommand"/>.</summary>
public sealed class VerifyAuditFindingCommandHandler : IRequestHandler<VerifyAuditFindingCommand, Result<VerifyAuditFindingResult>>
{
    private readonly IAuditFindingRepository _repository;
    private readonly ILogger<VerifyAuditFindingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyAuditFindingCommandHandler(IAuditFindingRepository repository, ILogger<VerifyAuditFindingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<VerifyAuditFindingResult>> Handle(VerifyAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditFindingId.FromGuid(request.AuditFindingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<VerifyAuditFindingResult>.Failure(new ApplicationError(
                    "Auditing.AuditFinding.NotFound",
                    "The requested AuditFinding record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Verify(
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<VerifyAuditFindingResult>.Success(new VerifyAuditFindingResult(
                aggregate.AuditFindingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify AuditFinding {AuditFindingId}", request.AuditFindingId);

            return Result<VerifyAuditFindingResult>.Failure(new ApplicationError(
                "Auditing.AuditFinding.VerifyFailed",
                "Unable to verify the AuditFinding at this time.",
                request.CorrelationId));
        }
    }
}
