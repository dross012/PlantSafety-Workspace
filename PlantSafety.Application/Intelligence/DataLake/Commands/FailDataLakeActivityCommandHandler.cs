// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           FailDataLakeActivityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for FailDataLakeActivityCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.DataLake.Interfaces;
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.DataLake.Commands;

/// <summary>Handles <see cref="FailDataLakeActivityCommand"/>.</summary>
public sealed class FailDataLakeActivityCommandHandler : IRequestHandler<FailDataLakeActivityCommand, Result<DataLakeActivityStatusResult>>
{
    private readonly IDataLakeActivityRepository _repository;
    private readonly ILogger<FailDataLakeActivityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FailDataLakeActivityCommandHandler(IDataLakeActivityRepository repository, ILogger<FailDataLakeActivityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeActivityStatusResult>> Handle(FailDataLakeActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DataLakeActivityId.FromGuid(request.DataLakeActivityId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DataLakeActivityStatusResult>.Failure(new ApplicationError(
                    "Intelligence.DataLake.NotFound",
                    "The requested data lake activity could not be found.",
                    request.DataLakeActivityId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Fail(
                failureReason: request.FailureReason,
                failedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DataLakeActivityStatusResult>.Success(new DataLakeActivityStatusResult(
                aggregate.DataLakeActivityId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark DataLakeActivity {DataLakeActivityId} as failed", request.DataLakeActivityId);

            return Result<DataLakeActivityStatusResult>.Failure(new ApplicationError(
                "Intelligence.DataLake.FailFailed",
                "Unable to mark the data lake activity as failed at this time.",
                Guid.NewGuid()));
        }
    }
}
