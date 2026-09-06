// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ContractTelemetryExtensionsTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Telemetry;

public sealed class ContractTelemetryExtensionsTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void ToLogPayload_ReturnsKeysInAlphabeticalOrder()
    {
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Clock.UtcNow);

        IReadOnlyDictionary<string, object?> payload = metadata.ToLogPayload();

        payload.Keys.Should().Equal("CallerRole", "CorrelationId", "MfaStatus", "SchemaVersion", "TenantId", "TimestampUtc");
    }

    [Fact]
    public void RecordEmitted_WritesContractMetadataEmittedLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Clock.UtcNow);

        metadata.RecordEmitted(Clock, writer);

        writer.LastEvent!.EventId.Should().Be("CONTRACT_METADATA_EMITTED");
        writer.LastEvent.Category.Should().Be(LogCategory.Infrastructure);
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }
}
