// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           ArchiveProceduresCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveProceduresCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Procedures.Interfaces;
using PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Handles <see cref="ArchiveProceduresCommand"/>.</summary>
public sealed class ArchiveProceduresCommandHandler : IRequestHandler<ArchiveProceduresCommand, Result<ProceduresStatusResult>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<ArchiveProceduresCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveProceduresCommandHandler(IProceduresRepository repository, ILogger<ArchiveProceduresCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProceduresStatusResult>> Handle(ArchiveProceduresCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProceduresId.FromGuid(request.ProceduresId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProceduresStatusResult>.Failure(new ApplicationError(
                    "Compliance.Procedures.NotFound",
                    "The requested procedure could not be found.",
                    request.ProceduresId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProceduresStatusResult>.Success(new ProceduresStatusResult(
                aggregate.ProceduresId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive Procedures {ProceduresId}", request.ProceduresId);

            return Result<ProceduresStatusResult>.Failure(new ApplicationError(
                "Compliance.Procedures.ArchiveFailed",
                "Unable to archive the procedure at this time.",
                Guid.NewGuid()));
        }
    }
}
