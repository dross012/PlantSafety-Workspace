// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Authorization;

public sealed class PredictiveAuthorizationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static PredictiveSafetyEvaluationRequestContract BuildRequest() => new(
        new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        "ChemicalSpill", null, 0, 0, 0, null, Guid.NewGuid(), null, 0, null, 0, 0, 0, 0, 0, Clock.UtcNow);

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_PlatformAdmin_Succeeds()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        Func<Task> act = () => PredictiveAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_ViewerRole_IsAlsoAuthorized()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        Func<Task> act = () => PredictiveAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-3");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        Func<Task> act = () => PredictiveAuthorization.EvaluateWithAuthorizationAsync(BuildRequest(), caller, denyingResolver, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
