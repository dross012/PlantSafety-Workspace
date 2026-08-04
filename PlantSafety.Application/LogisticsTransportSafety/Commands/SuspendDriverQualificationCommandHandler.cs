// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for SuspendDriverQualificationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="SuspendDriverQualificationCommand"/>.</summary>
public sealed class SuspendDriverQualificationCommandHandler : IRequestHandler<SuspendDriverQualificationCommand, Result<DriverQualificationActionResult>>
{
    private readonly IDriverQualificationRepository _repository;
    private readonly ILogger<SuspendDriverQualificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuspendDriverQualificationCommandHandler(IDriverQualificationRepository repository, ILogger<SuspendDriverQualificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DriverQualificationActionResult>> Handle(SuspendDriverQualificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DriverQualificationId.FromGuid(request.DriverQualificationId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<DriverQualificationActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.DriverQualifications.NotFound",
                    "The requested DriverQualification record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Suspend(
                request.Reason,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DriverQualificationActionResult>.Success(new DriverQualificationActionResult(
                aggregate.DriverQualificationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Suspend for DriverQualification {DriverQualificationId}", request.DriverQualificationId);

            return Result<DriverQualificationActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.DriverQualifications.SuspendFailed",
                "Unable to Suspend the DriverQualification record at this time.",
                Guid.NewGuid()));
        }
    }
}
