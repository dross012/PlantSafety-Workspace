// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for PublishMaterialHandlingStorageCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Interfaces;
using PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="PublishMaterialHandlingStorageCommand"/>.</summary>
public sealed class PublishMaterialHandlingStorageCommandHandler : IRequestHandler<PublishMaterialHandlingStorageCommand, Result<MaterialHandlingStorageActionResult>>
{
    private readonly IMaterialHandlingStorageRepository _repository;
    private readonly ILogger<PublishMaterialHandlingStorageCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishMaterialHandlingStorageCommandHandler(IMaterialHandlingStorageRepository repository, ILogger<PublishMaterialHandlingStorageCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MaterialHandlingStorageActionResult>> Handle(PublishMaterialHandlingStorageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MaterialHandlingStorageId.FromGuid(request.MaterialHandlingStorageId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MaterialHandlingStorageActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.MaterialHandlingStorage.NotFound",
                    "The requested MaterialHandlingStorage record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Publish(
                UserId.FromGuid(request.ApprovedByUserId),
                UtcTimestamp.Now(),
                request.ReviewCycleDays,
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MaterialHandlingStorageActionResult>.Success(new MaterialHandlingStorageActionResult(
                aggregate.MaterialHandlingStorageId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Publish for MaterialHandlingStorage {MaterialHandlingStorageId}", request.MaterialHandlingStorageId);

            return Result<MaterialHandlingStorageActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.MaterialHandlingStorage.PublishFailed",
                "Unable to Publish the MaterialHandlingStorage record at this time.",
                Guid.NewGuid()));
        }
    }
}