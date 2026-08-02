// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           StartAuditingCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartAuditingCommand. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException
//                  is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
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

/// <summary>Handles <see cref="StartAuditingCommand"/>.</summary>
public sealed class StartAuditingCommandHandler : IRequestHandler<StartAuditingCommand, Result<AuditingStatusResult>>
{
    private readonly IAuditingRepository _repository;
    private readonly ILogger<StartAuditingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartAuditingCommandHandler(IAuditingRepository repository, ILogger<StartAuditingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditingStatusResult>> Handle(StartAuditingCommand request, CancellationToken cancellationToken)
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

            var startedBy = UserId.FromGuid(request.StartedByUserId);

            aggregate.Start(
                startedBy: startedBy,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(startedBy),
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
            _logger.LogError(ex, "Failed to start Auditing {AuditingId}", request.AuditingId);

            return Result<AuditingStatusResult>.Failure(new ApplicationError(
                "Compliance.Auditing.StartFailed",
                "Unable to start the audit at this time.",
                Guid.NewGuid()));
        }
    }
}
