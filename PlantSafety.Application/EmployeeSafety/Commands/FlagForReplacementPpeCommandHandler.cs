// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for FlagForReplacementPpeCommand.
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

/// <summary>Handles <see cref="FlagForReplacementPpeCommand"/>.</summary>
public sealed class FlagForReplacementPpeCommandHandler : IRequestHandler<FlagForReplacementPpeCommand, Result<PpeActionResult>>
{
    private readonly IPpeRepository _repository;
    private readonly ILogger<FlagForReplacementPpeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagForReplacementPpeCommandHandler(IPpeRepository repository, ILogger<FlagForReplacementPpeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpeActionResult>> Handle(FlagForReplacementPpeCommand request, CancellationToken cancellationToken)
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

            aggregate.FlagForReplacement(
                request.Reason,
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
            _logger.LogError(ex, "Failed to execute FlagForReplacement for Ppe {PpeId}", request.PpeId);

            return Result<PpeActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.Ppe.FlagForReplacementFailed",
                "Unable to FlagForReplacement the Ppe record at this time.",
                Guid.NewGuid()));
        }
    }
}