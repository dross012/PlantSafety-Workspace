// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordTsdfReceiptCommand.
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

/// <summary>Handles <see cref="RecordTsdfReceiptCommand"/>.</summary>
public sealed class RecordTsdfReceiptCommandHandler : IRequestHandler<RecordTsdfReceiptCommand, Result<HazardousWasteManifestActionResult>>
{
    private readonly IHazardousWasteManifestRepository _repository;
    private readonly ILogger<RecordTsdfReceiptCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordTsdfReceiptCommandHandler(IHazardousWasteManifestRepository repository, ILogger<RecordTsdfReceiptCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardousWasteManifestActionResult>> Handle(RecordTsdfReceiptCommand request, CancellationToken cancellationToken)
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

            aggregate.RecordTsdfReceipt(
                request.TsdfFacilityId, UtcTimestamp.Now(), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<HazardousWasteManifestActionResult>.Success(new HazardousWasteManifestActionResult(
                aggregate.HazardousWasteManifestId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordTsdfReceipt for Manifest {HazardousWasteManifestId}", request.HazardousWasteManifestId);

            return Result<HazardousWasteManifestActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.Manifest.RecordTsdfReceiptFailed",
                "Unable to record TSDF receipt at this time.",
                Guid.NewGuid()));
        }
    }
}
