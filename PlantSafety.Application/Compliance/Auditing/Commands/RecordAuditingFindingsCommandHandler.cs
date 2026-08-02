// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           RecordAuditingFindingsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordAuditingFindingsCommand. Mirrors StartAuditingCommandHandler's
//                  tenant-scoped lookup / NotFound-severity / concurrency-propagation conventions.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Auditing.Interfaces;
using PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Auditing.Commands;

/// <summary>Handles <see cref="RecordAuditingFindingsCommand"/>.</summary>
public sealed class RecordAuditingFindingsCommandHandler : IRequestHandler<RecordAuditingFindingsCommand, Result<AuditingStatusResult>>
{
    private readonly IAuditingRepository _repository;
    private readonly ILogger<RecordAuditingFindingsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAuditingFindingsCommandHandler(IAuditingRepository repository, ILogger<RecordAuditingFindingsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditingStatusResult>> Handle(RecordAuditingFindingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditingId.FromGuid(request.AuditingId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AuditingStatusResult>.Failure(new ApplicationError(
                    "Compliance.Auditing.NotFound",
                    "The requested audit record could not be found.",
                    request.AuditingId,
                    ErrorSeverity.NotFound));
            }

            var completedBy = UserId.FromGuid(request.CompletedByUserId);

            aggregate.RecordFindings(
                completedBy: completedBy,
                findingsCount: request.FindingsCount,
                score: request.Score,
                complianceRating: request.ComplianceRating,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(completedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AuditingStatusResult>.Success(new AuditingStatusResult(
                aggregate.AuditingId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record findings for Auditing {AuditingId}", request.AuditingId);

            return Result<AuditingStatusResult>.Failure(new ApplicationError(
                "Compliance.Auditing.RecordFindingsFailed",
                "Unable to record findings for the audit at this time.",
                Guid.NewGuid()));
        }
    }
}
