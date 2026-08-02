// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           SupersedeProceduresCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for SupersedeProceduresCommand.
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

/// <summary>Handles <see cref="SupersedeProceduresCommand"/>.</summary>
public sealed class SupersedeProceduresCommandHandler : IRequestHandler<SupersedeProceduresCommand, Result<ProceduresStatusResult>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<SupersedeProceduresCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SupersedeProceduresCommandHandler(IProceduresRepository repository, ILogger<SupersedeProceduresCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProceduresStatusResult>> Handle(SupersedeProceduresCommand request, CancellationToken cancellationToken)
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

            aggregate.Supersede(
                supersededBy: performedBy,
                supersededAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to supersede Procedures {ProceduresId}", request.ProceduresId);

            return Result<ProceduresStatusResult>.Failure(new ApplicationError(
                "Compliance.Procedures.SupersedeFailed",
                "Unable to supersede the procedure at this time.",
                Guid.NewGuid()));
        }
    }
}
