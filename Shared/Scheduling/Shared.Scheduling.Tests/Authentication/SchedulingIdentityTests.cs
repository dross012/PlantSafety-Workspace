// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingIdentityTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Infrastructure.Authentication;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Scheduling.Tests.Authentication;

public sealed class SchedulingIdentityTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Resolve_NoApiKeyHeader_FallsBackToSystemDefaultsWithoutThrowing()
    {
        var writer = new CapturingLogEventWriter();

        SchedulingIdentityResolution resolution = SchedulingIdentity.Resolve(
            apiKeyHeaderValue: null,
            registeredApiKeys: new Dictionary<string, EnterpriseRole>(),
            registeredKeyTenants: new Dictionary<string, string>(),
            correlationId: "corr-1",
            Clock,
            writer);

        resolution.Caller.Should().NotBeNull();
        resolution.TenantContext.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_RecordsAuthenticationAndTenantTelemetry()
    {
        var writer = new CapturingLogEventWriter();

        SchedulingIdentity.Resolve(
            apiKeyHeaderValue: null,
            registeredApiKeys: new Dictionary<string, EnterpriseRole>(),
            registeredKeyTenants: new Dictionary<string, string>(),
            correlationId: "corr-2",
            Clock,
            writer);

        writer.Events.Should().NotBeEmpty();
    }
}
