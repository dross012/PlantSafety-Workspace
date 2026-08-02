// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           CurateDataLakeActivityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CurateDataLakeActivityCommand.
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

/// <summary>Handles <see cref="CurateDataLakeActivityCommand"/>.</summary>
public sealed class CurateDataLakeActivityCommandHandler : IRequestHandler<CurateDataLakeActivityCommand, Result<DataLakeActivityStatusResult>>
{
    private readonly IDataLakeActivityRepository _repository;
    private readonly ILogger<CurateDataLakeActivityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CurateDataLakeActivityCommandHandler(IDataLakeActivityRepository repository, ILogger<CurateDataLakeActivityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeActivityStatusResult>> Handle(CurateDataLakeActivityCommand request, CancellationToken cancellationToken)
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

            aggregate.Curate(
                finalRecordCount: request.FinalRecordCount,
                curatedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to curate DataLakeActivity {DataLakeActivityId}", request.DataLakeActivityId);

            return Result<DataLakeActivityStatusResult>.Failure(new ApplicationError(
                "Intelligence.DataLake.CurateFailed",
                "Unable to curate the data lake activity at this time.",
                Guid.NewGuid()));
        }
    }
}
