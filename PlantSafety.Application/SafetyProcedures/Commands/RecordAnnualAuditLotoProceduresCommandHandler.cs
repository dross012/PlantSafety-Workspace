// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordAnnualAuditLotoProceduresCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.LotoProcedures.Interfaces;
using PlantSafety.Domain.SafetyProcedures.LotoProcedures.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="RecordAnnualAuditLotoProceduresCommand"/>.</summary>
public sealed class RecordAnnualAuditLotoProceduresCommandHandler : IRequestHandler<RecordAnnualAuditLotoProceduresCommand, Result<LotoProceduresActionResult>>
{
    private readonly ILotoProceduresRepository _repository;
    private readonly ILogger<RecordAnnualAuditLotoProceduresCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAnnualAuditLotoProceduresCommandHandler(ILotoProceduresRepository repository, ILogger<RecordAnnualAuditLotoProceduresCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoProceduresActionResult>> Handle(RecordAnnualAuditLotoProceduresCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LotoProceduresId.FromGuid(request.LotoProceduresId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LotoProceduresActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.LotoProcedures.NotFound",
                    "The requested LotoProcedures record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordAnnualAudit(
                request.Compliant,
                request.ComplianceScore,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LotoProceduresActionResult>.Success(new LotoProceduresActionResult(
                aggregate.LotoProceduresId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordAnnualAudit for LotoProcedures {LotoProceduresId}", request.LotoProceduresId);

            return Result<LotoProceduresActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.LotoProcedures.RecordAnnualAuditFailed",
                "Unable to RecordAnnualAudit the LotoProcedures record at this time.",
                Guid.NewGuid()));
        }
    }
}