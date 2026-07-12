// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetDataLakeQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.DataLake.Queries;
using PlantSafety.Application.Intelligence.DataLake.Repositories;

namespace PlantSafety.Application.Intelligence.DataLake.Handlers;

/// <summary>Handles <see cref="GetDataLakeQuery"/>.</summary>
public sealed class GetDataLakeQueryHandler : IRequestHandler<GetDataLakeQuery, Result<DataLakeQueryResult>>
{
    private readonly IDataLakeRepository _repository;
    private readonly ILogger<GetDataLakeQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDataLakeQueryHandler(IDataLakeRepository repository, ILogger<GetDataLakeQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DataLakeQueryResult>> Handle(GetDataLakeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDataLakeRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<DataLakeQueryResult>.Success(new DataLakeQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DataLake data for Tenant {TenantId}", request.TenantId);

            return Result<DataLakeQueryResult>.Failure(new ApplicationError(
                "Intelligence.DataLake.RetrievalFailed",
                "Unable to retrieve DataLake data at this time.",
                Guid.NewGuid()));
        }
    }
}