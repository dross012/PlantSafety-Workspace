// ============================================================================
//  Project:        Shared.SafetyData.Tests
//  File:           RoutingAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyData.Tests.Authorization;

public sealed class RoutingAuthorizationTests
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
    public async Task RouteWithAuthorizationAsync_PlatformAdmin_MatchesDirectEngineResult()
    {
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition eventDefinition = EventCatalog.FindByCode("ChemicalSpill")!;

        EventRoutingResult viaWrapper = await RoutingAuthorization.RouteWithAuthorizationAsync(eventDefinition, facility, caller, Resolver, Clock, Writer);
        EventRoutingResult direct = EventRoutingEngine.Route(eventDefinition, facility);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task RouteWithAuthorizationAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition eventDefinition = EventCatalog.FindByCode("ChemicalSpill")!;

        Func<Task> act = () => RoutingAuthorization.RouteWithAuthorizationAsync(eventDefinition, facility, caller, new AlwaysDenyPermissionResolver(), Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
