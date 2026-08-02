// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           StartProcessingDataLakeActivityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartProcessingDataLakeActivityCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
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

/// <summary>Handles <see cref="StartProcessingDataLakeActivityCommand"/>.</summary>
public sealed class StartProcessingDataLakeActivityCommandHandler : IRequestHandler<StartProcessingDataLakeActivityCommand, Result<DataLakeActivityStatusResult>>
{
    private readonly IDataLakeActivityRepository _repository;
    private readonly ILogger<StartProcessingDataLakeActivityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartProcessingDataLakeActivityCommandHandler(IDataLakeActivityRepository repository, ILogger<StartProcessingDataLakeActivityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeActivityStatusResult>> Handle(StartProcessingDataLakeActivityCommand request, CancellationToken cancellationToken)
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

            var startedBy = UserId.FromGuid(request.StartedByUserId);

            aggregate.StartProcessing(
                startedBy: startedBy,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(startedBy),
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
            _logger.LogError(ex, "Failed to start processing DataLakeActivity {DataLakeActivityId}", request.DataLakeActivityId);

            return Result<DataLakeActivityStatusResult>.Failure(new ApplicationError(
                "Intelligence.DataLake.StartProcessingFailed",
                "Unable to start processing the data lake activity at this time.",
                Guid.NewGuid()));
        }
    }
}
