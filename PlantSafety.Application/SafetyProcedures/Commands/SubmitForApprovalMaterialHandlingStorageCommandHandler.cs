// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SubmitForApprovalMaterialHandlingStorageCommand.
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

/// <summary>Handles <see cref="SubmitForApprovalMaterialHandlingStorageCommand"/>.</summary>
public sealed class SubmitForApprovalMaterialHandlingStorageCommandHandler : IRequestHandler<SubmitForApprovalMaterialHandlingStorageCommand, Result<MaterialHandlingStorageActionResult>>
{
    private readonly IMaterialHandlingStorageRepository _repository;
    private readonly ILogger<SubmitForApprovalMaterialHandlingStorageCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForApprovalMaterialHandlingStorageCommandHandler(IMaterialHandlingStorageRepository repository, ILogger<SubmitForApprovalMaterialHandlingStorageCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MaterialHandlingStorageActionResult>> Handle(SubmitForApprovalMaterialHandlingStorageCommand request, CancellationToken cancellationToken)
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

            aggregate.SubmitForApproval(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MaterialHandlingStorageActionResult>.Success(new MaterialHandlingStorageActionResult(
                aggregate.MaterialHandlingStorageId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SubmitForApproval for MaterialHandlingStorage {MaterialHandlingStorageId}", request.MaterialHandlingStorageId);

            return Result<MaterialHandlingStorageActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.MaterialHandlingStorage.SubmitForApprovalFailed",
                "Unable to SubmitForApproval the MaterialHandlingStorage record at this time.",
                Guid.NewGuid()));
        }
    }
}