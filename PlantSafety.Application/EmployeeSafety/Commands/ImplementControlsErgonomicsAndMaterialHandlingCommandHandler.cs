// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ImplementControlsErgonomicsAndMaterialHandlingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Interfaces;
using PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="ImplementControlsErgonomicsAndMaterialHandlingCommand"/>.</summary>
public sealed class ImplementControlsErgonomicsAndMaterialHandlingCommandHandler : IRequestHandler<ImplementControlsErgonomicsAndMaterialHandlingCommand, Result<ErgonomicsAndMaterialHandlingActionResult>>
{
    private readonly IErgonomicsAndMaterialHandlingRepository _repository;
    private readonly ILogger<ImplementControlsErgonomicsAndMaterialHandlingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ImplementControlsErgonomicsAndMaterialHandlingCommandHandler(IErgonomicsAndMaterialHandlingRepository repository, ILogger<ImplementControlsErgonomicsAndMaterialHandlingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsAndMaterialHandlingActionResult>> Handle(ImplementControlsErgonomicsAndMaterialHandlingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ErgonomicsAndMaterialHandlingId.FromGuid(request.ErgonomicsAndMaterialHandlingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<ErgonomicsAndMaterialHandlingActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.ErgonomicsAndMaterialHandling.NotFound",
                    "The requested ErgonomicsAndMaterialHandling record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ImplementControls(
                request.ControlsDescription,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ErgonomicsAndMaterialHandlingActionResult>.Success(new ErgonomicsAndMaterialHandlingActionResult(
                aggregate.ErgonomicsAndMaterialHandlingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ImplementControls for ErgonomicsAndMaterialHandling {ErgonomicsAndMaterialHandlingId}", request.ErgonomicsAndMaterialHandlingId);

            return Result<ErgonomicsAndMaterialHandlingActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.ErgonomicsAndMaterialHandling.ImplementControlsFailed",
                "Unable to ImplementControls the ErgonomicsAndMaterialHandling record at this time.",
                Guid.NewGuid()));
        }
    }
}