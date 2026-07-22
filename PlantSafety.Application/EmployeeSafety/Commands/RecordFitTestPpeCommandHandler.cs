// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordFitTestPpeCommand.
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

/// <summary>Handles <see cref="RecordFitTestPpeCommand"/>.</summary>
public sealed class RecordFitTestPpeCommandHandler : IRequestHandler<RecordFitTestPpeCommand, Result<PpeActionResult>>
{
    private readonly IPpeRepository _repository;
    private readonly ILogger<RecordFitTestPpeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordFitTestPpeCommandHandler(IPpeRepository repository, ILogger<RecordFitTestPpeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpeActionResult>> Handle(RecordFitTestPpeCommand request, CancellationToken cancellationToken)
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

            aggregate.RecordFitTest(
                request.Passed,
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
            _logger.LogError(ex, "Failed to execute RecordFitTest for Ppe {PpeId}", request.PpeId);

            return Result<PpeActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.Ppe.RecordFitTestFailed",
                "Unable to RecordFitTest the Ppe record at this time.",
                Guid.NewGuid()));
        }
    }
}