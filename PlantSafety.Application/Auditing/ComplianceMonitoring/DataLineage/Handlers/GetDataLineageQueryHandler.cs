// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetDataLineageQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Handlers;

/// <summary>Handles <see cref="GetDataLineageQuery"/>.</summary>
public sealed class GetDataLineageQueryHandler(
    IDataLineageRepository repository,
    ILogger<GetDataLineageQueryHandler> logger)
    : IRequestHandler<GetDataLineageQuery, Result<DataLineageQueryResult>>
{
    /// <inheritdoc/>
    public async Task<Result<DataLineageQueryResult>> Handle(GetDataLineageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entries = await repository.GetLineageAsync(request.TenantId, request.CorrelationId, cancellationToken);

            return Result<DataLineageQueryResult>.Success(new DataLineageQueryResult(
                request.CorrelationId,
                DateTime.UtcNow,
                entries));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconstruct data lineage for Tenant {TenantId} CorrelationId {CorrelationId}", request.TenantId, request.CorrelationId);

            return Result<DataLineageQueryResult>.Failure(new ApplicationError(
                "Auditing.ComplianceMonitoring.DataLineage.RetrievalFailed",
                "Unable to reconstruct data lineage at this time.",
                Guid.NewGuid()));
        }
    }
}
