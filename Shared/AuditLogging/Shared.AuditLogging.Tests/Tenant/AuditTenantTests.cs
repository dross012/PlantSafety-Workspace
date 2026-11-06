// ============================================================================
//  Project:        Shared.AuditLogging.Tests
//  File:           AuditTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.Tenant;
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.AuditLogging.Tests.Tenant;

public sealed class AuditTenantTests
{
    private sealed class FakeAuditLogRepository(AuditLogSearchResult result) : IAuditLogRepository
    {
        public bool WasCalled { get; private set; }

        public Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FakeResolver(bool canAccess) : ITenantFacilityAccessResolver
    {
        public bool CanAccessFacility(string tenantId, Guid facilityId) => canAccess;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public async Task SearchAsync_UnscopedTenant_DelegatesToInnerRepository()
    {
        var expected = new AuditLogSearchResult([], 1, 50, 0);
        var inner = new FakeAuditLogRepository(expected);
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");
        var repository = new TenantScopedAuditLogRepository(inner, tenantContext, requestedFacilityId: null, tenantFacilityAccessResolver: null, Clock, new NoOpLogEventWriter());

        AuditLogSearchResult result = await repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        result.Should().Be(expected);
        inner.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessExceptionWithoutCallingInner()
    {
        var inner = new FakeAuditLogRepository(new AuditLogSearchResult([], 1, 50, 0));
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-2");
        var repository = new TenantScopedAuditLogRepository(inner, tenantContext, requestedFacilityId: Guid.NewGuid(), tenantFacilityAccessResolver: null, Clock, new NoOpLogEventWriter());

        Func<Task> act = () => repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        inner.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_ResolverGrantsAccess_DelegatesToInnerRepository()
    {
        var expected = new AuditLogSearchResult([], 1, 50, 0);
        var inner = new FakeAuditLogRepository(expected);
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-3");
        var repository = new TenantScopedAuditLogRepository(inner, tenantContext, requestedFacilityId: Guid.NewGuid(), new FakeResolver(true), Clock, new NoOpLogEventWriter());

        AuditLogSearchResult result = await repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        result.Should().Be(expected);
        inner.WasCalled.Should().BeTrue();
    }
}
