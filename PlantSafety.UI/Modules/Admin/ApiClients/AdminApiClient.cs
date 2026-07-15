// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Administrative Setup page.
//   Wires the Users & Roles, Feature Flags/System Options, and Audit Log
//   tabs to the existing PlantSafety.Api read-model endpoints:
//     GET v1/cross-domain/users          (Users & Roles tab — users)
//     GET v1/cross-domain/roles          (Users & Roles tab — roles)
//     GET v1/cross-domain/permissions    (Users & Roles tab — permission matrix)
//     GET v1/cross-domain/administration (Feature Flags / System Options tab)
//     GET v1/integrations/auditing       (Audit Log tab)
//   These backend endpoints are currently GET-only read-model scaffolds —
//   there are no POST/PUT command endpoints yet for role assignment,
//   permission changes, or system-option edits. AdminSetupPage therefore
//   renders the corresponding write actions in a disabled state with a
//   "needs backend wiring" indicator rather than silently failing.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>
/// Typed API client for the Administrative Setup page. All methods return
/// <see cref="ApiResponseEnvelope{T}"/> — callers MUST check <c>IsSuccess</c>
/// before accessing <c>Data</c>.
/// </summary>
public sealed class AdminApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminApiClient"/>.
    /// </summary>
    /// <param name="http">The typed HttpClient with correlation and auth handlers.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AdminApiClient(HttpClient http, ILogger<AdminApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Fetches the tenant's user directory for the Users &amp; Roles tab.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<AdminListResponseDto<AdminUserDto>>> GetUsersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => GetAsync<AdminListResponseDto<AdminUserDto>>(
            $"v1/cross-domain/users?{BuildDateRangeQuery(tenantId)}",
            cancellationToken);

    /// <summary>Fetches the tenant's security roles for the Users &amp; Roles tab.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<AdminListResponseDto<AdminRoleDto>>> GetRolesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => GetAsync<AdminListResponseDto<AdminRoleDto>>(
            $"v1/cross-domain/roles?{BuildDateRangeQuery(tenantId)}",
            cancellationToken);

    /// <summary>Fetches the tenant's permission grants for the Permission Matrix panel.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<AdminListResponseDto<AdminPermissionDto>>> GetPermissionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => GetAsync<AdminListResponseDto<AdminPermissionDto>>(
            $"v1/cross-domain/permissions?{BuildDateRangeQuery(tenantId)}",
            cancellationToken);

    /// <summary>Fetches the tenant's system configuration / feature flag records for the Feature Flags tab.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<AdminListResponseDto<AdminSettingDto>>> GetSettingsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => GetAsync<AdminListResponseDto<AdminSettingDto>>(
            $"v1/cross-domain/administration?{BuildDateRangeQuery(tenantId)}",
            cancellationToken);

    /// <summary>Fetches integration-layer audit log entries for the Audit Log tab.</summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<AdminListResponseDto<AdminAuditLogEntryDto>>> GetAuditLogAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => GetAsync<AdminListResponseDto<AdminAuditLogEntryDto>>(
            $"v1/integrations/auditing?{BuildDateRangeQuery(tenantId)}",
            cancellationToken);

    /// <summary>
    /// Builds the <c>TenantId</c>/<c>From</c>/<c>To</c> query string shared by every
    /// CrossDomain/Integrations read-model endpoint. Uses a rolling 90-day window
    /// since these reporting-style endpoints require an explicit date range.
    /// </summary>
    private static string BuildDateRangeQuery(Guid tenantId)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-90);

        var tenantIdSegment = Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture));
        var fromSegment = Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture));
        var toSegment = Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture));

        return $"TenantId={tenantIdSegment}&From={fromSegment}&To={toSegment}";
    }
}
