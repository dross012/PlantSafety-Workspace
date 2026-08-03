// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal Support Console's tenant audit
//   stream viewer. Mirrors PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage
//   read models and the new ComplianceMonitoringV1Controller endpoint.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>A single authorization (permission-check) decision, allow or deny (mirrors <c>PermissionUsageLogEntry</c>).</summary>
public sealed class PermissionUsageLogEntryDto
{
    /// <summary>Gets the tenant the request was scoped to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the authenticated user the decision was evaluated for.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the fully-qualified permission name that was checked.</summary>
    public string Permission { get; init; } = string.Empty;

    /// <summary>Gets the MediatR request type name that triggered the check.</summary>
    public string RequestType { get; init; } = string.Empty;

    /// <summary>Gets "Allowed" or "Denied".</summary>
    public string Decision { get; init; } = string.Empty;

    /// <summary>Gets the request's correlation identifier.</summary>
    public Guid CorrelationId { get; init; }

    /// <summary>Gets the UTC instant the decision was made.</summary>
    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>Result of the tenant audit stream query (mirrors <c>TenantAuditStreamResult</c>).</summary>
public sealed class TenantAuditStreamResultDto
{
    /// <summary>Gets the tenant this stream was scoped to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the UTC timestamp when this stream snapshot was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the tenant's authorization-decision entries, newest first.</summary>
    public IReadOnlyList<PermissionUsageLogEntryDto> Entries { get; init; } = [];
}
