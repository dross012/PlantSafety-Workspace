// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 7 §5.4)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetTenantAuditStreamQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Handlers;

/// <summary>Handles <see cref="GetTenantAuditStreamQuery"/>.</summary>
public sealed class GetTenantAuditStreamQueryHandler(
    IPermissionUsageLogReadRepository repository,
    ILogger<GetTenantAuditStreamQueryHandler> logger)
    : IRequestHandler<GetTenantAuditStreamQuery, Result<TenantAuditStreamResult>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantAuditStreamResult>> Handle(GetTenantAuditStreamQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entries = await repository.GetByTenantAsync(request.TenantId, request.FromUtc, request.ToUtc, cancellationToken);

            return Result<TenantAuditStreamResult>.Success(new TenantAuditStreamResult(
                request.TenantId,
                DateTime.UtcNow,
                entries));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve tenant audit stream for Tenant {TenantId}", request.TenantId);

            return Result<TenantAuditStreamResult>.Failure(new ApplicationError(
                "Auditing.ComplianceMonitoring.TenantAuditStream.RetrievalFailed",
                "Unable to retrieve the tenant audit stream at this time.",
                Guid.NewGuid()));
        }
    }
}
