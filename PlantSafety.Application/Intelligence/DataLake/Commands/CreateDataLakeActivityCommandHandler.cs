// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           CreateDataLakeActivityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateDataLakeActivityCommand. Pure orchestration: constructs the
//                  DataLakeActivity aggregate via its domain factory method and persists it through
//                  the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.DataLake.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.DataLake.Commands;

/// <summary>Handles <see cref="CreateDataLakeActivityCommand"/>.</summary>
public sealed class CreateDataLakeActivityCommandHandler : IRequestHandler<CreateDataLakeActivityCommand, Result<DataLakeActivityStatusResult>>
{
    private readonly IDataLakeActivityRepository _repository;
    private readonly ILogger<CreateDataLakeActivityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateDataLakeActivityCommandHandler(IDataLakeActivityRepository repository, ILogger<CreateDataLakeActivityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeActivityStatusResult>> Handle(CreateDataLakeActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var processedBy = UserId.FromGuid(request.ProcessedByUserId);

            var aggregate = PlantSafety.Domain.Intelligence.DataLake.Aggregates.DataLakeActivity.Create(
                createdBy: processedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(processedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                datasetName: request.DatasetName,
                zone: request.Zone,
                recordCount: request.RecordCount,
                processedBy: processedBy);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DataLakeActivityStatusResult>.Success(new DataLakeActivityStatusResult(
                aggregate.DataLakeActivityId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DataLakeActivity for Tenant {TenantId}", request.TenantId);

            return Result<DataLakeActivityStatusResult>.Failure(new ApplicationError(
                "Intelligence.DataLake.CreateFailed",
                "Unable to create the data lake activity at this time.",
                Guid.NewGuid()));
        }
    }
}
