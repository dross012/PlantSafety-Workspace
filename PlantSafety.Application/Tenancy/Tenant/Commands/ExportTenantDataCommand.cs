// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to export a tenant's own canonical record set (Phase 7 §2.3 Tenant
//   Lifecycle Management). SCOPE NOTE: exports the Tenant aggregate's own canonical record
//   (identity, region, plan, status, configuration -- i.e. the Tenancy bounded context's own
//   data) as JSON, reusing the platform's existing IBlobStorageService the same way
//   AuditReportBlobContentStore does. This is deliberately scoped to the Tenant registry's own
//   data, not a literal dump of all ~150+ tenant-scoped business tables across the platform --
//   see the Handbook's own canonical-model direction (referenced in
//   PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh.EnterpriseEventEnvelope's header) for
//   where a full cross-module canonical-DTO export would eventually plug in once that work
//   lands; fabricating ad hoc per-module export logic here would duplicate that future
//   canonical-model surface rather than build on it. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Exports a tenant's own canonical Tenancy-context record set as a downloadable JSON artifact.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TargetTenantId">Identity of the tenant whose record should be exported.</param>
/// <param name="ActorId">Identity of the platform administrator requesting export.</param>
public sealed record ExportTenantDataCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TargetTenantId,
    Guid ActorId) : ICommand<Result<TenantDataExportResultDto>>
{
    /// <summary>Platform-admin, cross-tenant lifecycle operation -- see <see cref="SuspendTenantCommand"/>'s equivalent member remarks for the full rationale.</summary>
    Guid ICommand<Result<TenantDataExportResultDto>>.TenantId => TargetTenantId;

    /// <inheritdoc/>
    Guid? ICommand<Result<TenantDataExportResultDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static ExportTenantDataCommand FromDto(
        Guid targetTenantId, Guid actorId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken, targetTenantId, actorId);
}

/// <summary>Outcome of a completed tenant data export.</summary>
/// <param name="ContentReference">Storage reference (blob URI) of the exported JSON artifact -- the export's destination.</param>
/// <param name="ContentSizeBytes">Size, in bytes, of the exported artifact.</param>
/// <param name="ExportedAtUtc">UTC instant the export completed.</param>
/// <param name="ValidatedRegionCode">
/// Region code (<see cref="PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion.Region"/>) the
/// platform's current deployment region was validated against by <c>IDataResidencyEnforcer</c>
/// immediately before this export was written -- completes the tenant -&gt; region -&gt; destination
/// lineage chain for compliance/audit review (Phase 9 §5.3 Export Lineage Validation). Added
/// additively; existing callers positionally constructing this record before this field are
/// unaffected only if using named/init construction -- see <c>ExportTenantDataCommandHandler</c>
/// for the sole real production construction site.
/// </param>
public sealed record TenantDataExportResultDto(
    string ContentReference,
    long ContentSizeBytes,
    DateTime ExportedAtUtc,
    string ValidatedRegionCode);
