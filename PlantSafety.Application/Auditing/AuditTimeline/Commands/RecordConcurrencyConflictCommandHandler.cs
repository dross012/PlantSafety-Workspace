// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for RecordConcurrencyConflictCommand. Pure orchestration: constructs
//   the ConcurrencyConflictRecord aggregate via its domain factory and persists it. No business
//   rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditTimeline.Commands;

/// <summary>Handles <see cref="RecordConcurrencyConflictCommand"/>.</summary>
public sealed class RecordConcurrencyConflictCommandHandler : IRequestHandler<RecordConcurrencyConflictCommand, Result<Guid>>
{
    private readonly IConcurrencyConflictFactory _factory;
    private readonly IConcurrencyConflictRepository _repository;
    private readonly ILogger<RecordConcurrencyConflictCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordConcurrencyConflictCommandHandler(
        IConcurrencyConflictFactory factory, IConcurrencyConflictRepository repository, ILogger<RecordConcurrencyConflictCommandHandler> logger)
    {
        _factory = factory;
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(RecordConcurrencyConflictCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);

            var record = _factory.Capture(
                request.TenantId, request.EntityType, request.EntityId, request.ExpectedETag, request.ActualETag,
                request.AttemptedOperation, request.ActorId, UtcTimestamp.Now(),
                PerformedByActor.User(actorId), new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.AddAsync(record, cancellationToken);

            return Result<Guid>.Success(record.ConcurrencyConflictRecordId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record concurrency conflict for {EntityType} {EntityId} Tenant {TenantId}",
                request.EntityType, request.EntityId, request.TenantId);

            return Result<Guid>.Failure(new ApplicationError(
                "Auditing.AuditTimeline.ConcurrencyConflictRecordFailed",
                "Unable to record the concurrency conflict at this time.",
                Guid.NewGuid()));
        }
    }
}
