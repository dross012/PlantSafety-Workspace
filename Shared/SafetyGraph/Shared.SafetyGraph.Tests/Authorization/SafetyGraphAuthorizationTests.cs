// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Authorization;

public sealed class SafetyGraphAuthorizationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static SafetyGraphEvaluationRequestContract BuildRequest() => new(
        new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        new SafetyGraphConstructionRequestContract(
            new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
            Guid.NewGuid(), "Plant One", null, Guid.NewGuid(), null, null, null, null));

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_PlatformAdmin_Succeeds()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        Func<Task> act = () => SafetyGraphAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_ViewerRole_IsAlsoAuthorized()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        Func<Task> act = () => SafetyGraphAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-3");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        Func<Task> act = () => SafetyGraphAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, denyingResolver, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
