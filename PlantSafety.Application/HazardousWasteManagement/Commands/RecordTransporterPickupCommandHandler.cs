// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordTransporterPickupCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="RecordTransporterPickupCommand"/>.</summary>
public sealed class RecordTransporterPickupCommandHandler : IRequestHandler<RecordTransporterPickupCommand, Result<HazardousWasteManifestActionResult>>
{
    private readonly IHazardousWasteManifestRepository _repository;
    private readonly ILogger<RecordTransporterPickupCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordTransporterPickupCommandHandler(IHazardousWasteManifestRepository repository, ILogger<RecordTransporterPickupCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardousWasteManifestActionResult>> Handle(RecordTransporterPickupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazardousWasteManifestId.FromGuid(request.HazardousWasteManifestId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazardousWasteManifestActionResult>.Failure(new ApplicationError(
                    "HazardousWasteManagement.Manifest.NotFound",
                    "The requested manifest could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordTransporterPickup(
                request.TransporterId, UtcTimestamp.Now(), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<HazardousWasteManifestActionResult>.Success(new HazardousWasteManifestActionResult(
                aggregate.HazardousWasteManifestId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordTransporterPickup for Manifest {HazardousWasteManifestId}", request.HazardousWasteManifestId);

            return Result<HazardousWasteManifestActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.Manifest.RecordTransporterPickupFailed",
                "Unable to record transporter pickup at this time.",
                Guid.NewGuid()));
        }
    }
}
