// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Tenancy
//  Layer:          IntegrationTests
//  File:           TenantAuditStreamAndCorrelationIsolationTests.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.4)
//  SchemaVersion:  2026.1
//  Description:    Proves two things end-to-end through the real DI-resolved MediatR pipeline and
//                  the real [Auditing].[PermissionUsageLog] table:
//                    1. GetTenantAuditStreamQuery is genuinely queryable per-tenant -- it never
//                       returns another tenant's access-log entries, even when both tenants have
//                       entries in the same physical table and share an overlapping time window.
//                    2. Cross-tenant correlation-id leakage prevention: a CorrelationId recorded
//                       against Tenant B's authorization decisions is never returned by, or
//                       discoverable through, Tenant A's audit-stream query -- CorrelationIds are
//                       request-scoped identifiers, not a cross-tenant lookup key, and the tenant
//                       audit stream must not become a side channel for enumerating another
//                       tenant's correlation identifiers.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using MediatR;

namespace PlantSafety.IntegrationTests.Tenancy;

/// <summary>
/// Proves <c>GetTenantAuditStreamQuery</c> is genuinely tenant-scoped and that CorrelationIds
/// never leak across tenant boundaries through the audit stream.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class TenantAuditStreamAndCorrelationIsolationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static readonly Guid TenantA = new("eeeeeeee-5555-5555-5555-555555555555");
    private static readonly Guid TenantB = new("ffffffff-6666-6666-6666-666666666666");

    [Fact(DisplayName = "PLNT-SEC-AUDIT-STREAM-001: GetTenantAuditStreamQuery never returns another tenant's entries, and never leaks another tenant's CorrelationId")]
    public async Task GetTenantAuditStreamQuery_ReturnsOnlyOwnTenantsEntries_AndNeverLeaksAnotherTenantsCorrelationId()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantA);
        var writeRepository = scope.ServiceProvider.GetRequiredService<IPermissionUsageLogRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var now = DateTime.UtcNow;
        var tenantACorrelationId = Guid.NewGuid();
        var tenantBCorrelationId = Guid.NewGuid();

        await writeRepository.RecordAsync(new PermissionUsageLogEntry(
            TenantId: TenantA,
            UserId: Guid.NewGuid(),
            Permission: "accidents.read",
            RequestType: nameof(GetTenantAuditStreamQuery),
            Decision: "Allowed",
            CorrelationId: tenantACorrelationId,
            OccurredAtUtc: now), CancellationToken.None);

        await writeRepository.RecordAsync(new PermissionUsageLogEntry(
            TenantId: TenantB,
            UserId: Guid.NewGuid(),
            Permission: "accidents.read",
            RequestType: nameof(GetTenantAuditStreamQuery),
            Decision: "Denied",
            CorrelationId: tenantBCorrelationId,
            OccurredAtUtc: now), CancellationToken.None);

        var query = new GetTenantAuditStreamQuery(TenantA, now.AddMinutes(-5), now.AddMinutes(5));
        var result = await sender.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TenantA);
        result.Value.Entries.Should().NotBeEmpty();

        result.Value.Entries.Should().OnlyContain(e => e.TenantId == TenantA,
            "the tenant audit stream must never contain another tenant's entries, even though both tenants " +
            "recorded decisions in the same overlapping time window against the same physical table");

        result.Value.Entries.Should().NotContain(e => e.CorrelationId == tenantBCorrelationId,
            "Tenant B's CorrelationId must never be observable through Tenant A's audit stream -- " +
            "correlation identifiers are request-scoped, not a cross-tenant enumeration key");

        result.Value.Entries.Should().Contain(e => e.CorrelationId == tenantACorrelationId,
            "Tenant A's own recorded decision must be present in its own audit stream");
    }

    [Fact(DisplayName = "PLNT-SEC-AUDIT-STREAM-002: Tenant B's own audit-stream query symmetrically never returns Tenant A's entries")]
    public async Task GetTenantAuditStreamQuery_IsSymmetric_TenantBNeverSeesTenantAsEntries()
    {
        using var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantB);
        var writeRepository = scope.ServiceProvider.GetRequiredService<IPermissionUsageLogRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var now = DateTime.UtcNow;

        await writeRepository.RecordAsync(new PermissionUsageLogEntry(
            TenantA, Guid.NewGuid(), "workorders.write", "SymmetryProbe", "Allowed", Guid.NewGuid(), now), CancellationToken.None);
        await writeRepository.RecordAsync(new PermissionUsageLogEntry(
            TenantB, Guid.NewGuid(), "workorders.write", "SymmetryProbe", "Allowed", Guid.NewGuid(), now), CancellationToken.None);

        var result = await sender.Send(new GetTenantAuditStreamQuery(TenantB, now.AddMinutes(-5), now.AddMinutes(5)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Entries.Should().OnlyContain(e => e.TenantId == TenantB);
    }
}
