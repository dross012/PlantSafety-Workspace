// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordInspectionPpeCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.Ppe.Interfaces;
using PlantSafety.Domain.EmployeeSafety.Ppe.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="RecordInspectionPpeCommand"/>.</summary>
public sealed class RecordInspectionPpeCommandHandler : IRequestHandler<RecordInspectionPpeCommand, Result<PpeActionResult>>
{
    private readonly IPpeRepository _repository;
    private readonly ILogger<RecordInspectionPpeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordInspectionPpeCommandHandler(IPpeRepository repository, ILogger<RecordInspectionPpeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpeActionResult>> Handle(RecordInspectionPpeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PpeId.FromGuid(request.PpeId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PpeActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.Ppe.NotFound",
                    "The requested Ppe record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordInspection(
                request.ConditionScore,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PpeActionResult>.Success(new PpeActionResult(
                aggregate.PpeId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordInspection for Ppe {PpeId}", request.PpeId);

            return Result<PpeActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.Ppe.RecordInspectionFailed",
                "Unable to RecordInspection the Ppe record at this time.",
                Guid.NewGuid()));
        }
    }
}