// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphIdentityTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyGraph.Infrastructure.Authentication;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Authentication;

public sealed class SafetyGraphIdentityTests
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

        SafetyGraphIdentityResolution resolution = SafetyGraphIdentity.Resolve(
            null, new Dictionary<string, EnterpriseRole>(), new Dictionary<string, string>(), "corr-1", Clock, writer);

        resolution.Caller.Should().NotBeNull();
        resolution.TenantContext.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_RecordsTelemetry()
    {
        var writer = new CapturingLogEventWriter();

        SafetyGraphIdentity.Resolve(null, new Dictionary<string, EnterpriseRole>(), new Dictionary<string, string>(), "corr-2", Clock, writer);

        writer.Events.Should().NotBeEmpty();
    }
}
