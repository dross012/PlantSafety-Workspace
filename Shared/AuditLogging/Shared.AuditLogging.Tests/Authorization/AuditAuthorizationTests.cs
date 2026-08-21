// ============================================================================
//  Project:        Shared.AuditLogging.Tests
//  File:           AuditAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.Authorization;
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.AuditLogging.Tests.Authorization;

public sealed class AuditAuthorizationTests
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

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();

    [Fact]
    public async Task SearchAsync_PlatformAdmin_DelegatesToInnerRepositoryAndReturnsItsResult()
    {
        var expected = new AuditLogSearchResult([], 1, 50, 0);
        var inner = new FakeAuditLogRepository(expected);
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");
        var repository = new AuthorizedAuditLogRepository(inner, caller, Resolver, Clock, new NoOpLogEventWriter());

        AuditLogSearchResult result = await repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        result.Should().Be(expected);
        inner.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_DeniedCaller_ThrowsUnauthorizedAccessExceptionWithoutCallingInner()
    {
        var inner = new FakeAuditLogRepository(new AuditLogSearchResult([], 1, 50, 0));
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");
        var repository = new AuthorizedAuditLogRepository(inner, caller, new AlwaysDenyPermissionResolver(), Clock, new NoOpLogEventWriter());

        Func<Task> act = () => repository.SearchAsync(new AuditLogSearchCriteria(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        inner.WasCalled.Should().BeFalse();
    }
}
