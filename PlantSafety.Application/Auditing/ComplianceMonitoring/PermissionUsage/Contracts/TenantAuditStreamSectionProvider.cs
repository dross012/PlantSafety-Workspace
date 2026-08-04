// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: PermissionUsage's implementation of the SharedKernel
//   IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult> contract.
//   Dispatches GetTenantAuditStreamQuery via ISender -- fully permitted because the dispatch stays
//   entirely inside this submodule -- and adapts the Application-layer Result<T>/ApplicationError
//   into the SharedKernel's canonical Result<T>/Error so that the sibling ComplianceReport
//   submodule can compose the audit-trail/incident-lineage section without referencing
//   PermissionUsage.Queries at all. Registered by ComplianceMonitoringApplicationServiceRegistration.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries.TenantAuditStreamResult>;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Contracts;

/// <summary>
/// PermissionUsage's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing the tenant-scoped authorization-decision/audit-trail stream (the platform's
/// general-purpose lineage record for every state-changing operation, including incident lifecycle
/// transitions -- see <c>AuthorizationBehavior</c>) as a public cross-module read-model contract.
/// </summary>
public sealed class TenantAuditStreamSectionProvider : IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public TenantAuditStreamSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "TenantAuditStream";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(TenantAuditStreamSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetTenantAuditStreamQuery(request.TenantId, request.FromUtc, request.ToUtc), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}
