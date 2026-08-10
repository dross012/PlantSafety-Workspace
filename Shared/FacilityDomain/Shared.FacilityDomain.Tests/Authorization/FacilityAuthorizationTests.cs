// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.FacilityDomain.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.FacilityDomain.Tests.Authorization;

public sealed class FacilityAuthorizationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
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

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly ILogEventWriter Writer = new CapturingLogEventWriter();

    [Fact]
    public async Task InferHazardTypeWithAuthorizationAsync_PlatformAdmin_MatchesDirectRuleResult()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        HazardType viaWrapper = await FacilityAuthorization.InferHazardTypeWithAuthorizationAsync("Fire in warehouse", null, caller, Resolver, Clock, Writer);
        HazardType direct = HazardInferenceRules.InferHazardTypeFromSafetyEvent("Fire in warehouse", null);

        viaWrapper.Should().Be(direct);
    }

    [Fact]
    public async Task InferHazardTypeWithAuthorizationAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        Func<Task> act = () => FacilityAuthorization.InferHazardTypeWithAuthorizationAsync("Fire", null, caller, new AlwaysDenyPermissionResolver(), Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task InferSeverityWithAuthorizationAsync_PlatformAdmin_MatchesDirectRuleResult()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-3");

        SeverityLevel viaWrapper = await FacilityAuthorization.InferSeverityWithAuthorizationAsync(HazardType.Fire, 2, 1, 3, caller, Resolver, Clock, Writer);
        SeverityLevel direct = SeverityInferenceRules.Infer(HazardType.Fire, 2, 1, 3);

        viaWrapper.Should().Be(direct);
    }

    [Fact]
    public async Task InferRegulatoryJurisdictionsWithAuthorizationAsync_PlatformAdmin_MatchesDirectRuleResult()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-4");

        var viaWrapper = await FacilityAuthorization.InferRegulatoryJurisdictionsWithAuthorizationAsync(HazardType.Fire, SeverityLevel.Severe, caller, Resolver, Clock, Writer);
        var direct = RegulatoryInferenceRules.Infer(HazardType.Fire, SeverityLevel.Severe);

        viaWrapper.Should().BeEquivalentTo(direct);
    }
}
