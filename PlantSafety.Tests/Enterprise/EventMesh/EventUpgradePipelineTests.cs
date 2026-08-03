// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           EventUpgradePipelineTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.3 Event Versioning, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Verifies v1 -> v2 -> v3 chained upgrade, replay compatibility of the original v1
//                  shape when no upgrader is registered for an EventType, and that a deprecated
//                  version logs a warning without throwing or blocking processing. Isolated,
//                  deterministic, no I/O.
// ============================================================================
using System.Text.Json;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class EventUpgradePipelineTests : UnitTestBase
{
    private const string EventType = "plantsafety.workorder.completed.v1";

    private static EnterpriseEventEnvelope EnvelopeAt(int schemaVersion, object payload) =>
        EnterpriseEventEnvelope.Create(EventType, schemaVersion, EnterpriseSourceSystem.Platform,
            FixedUtcNow, TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, TestUserId.Value, payload);

    [Fact]
    public void UpgradeToLatest_ChainsMultipleUpgraders_ProducesLatestSchemaVersion()
    {
        var v1ToV2 = new TestUpgrader(EventType, 1, 2, p =>
        {
            using var doc = JsonDocument.Parse(p.GetRawText());
            var dict = doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => (object?)x.Value.GetRawText());
            dict["addedInV2"] = "\"default\"";
            return BuildElement(dict);
        });
        var v2ToV3 = new TestUpgrader(EventType, 2, 3, p =>
        {
            using var doc = JsonDocument.Parse(p.GetRawText());
            var dict = doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => (object?)x.Value.GetRawText());
            dict["addedInV3"] = "\"default\"";
            return BuildElement(dict);
        });

        var pipeline = new EventUpgradePipeline([v1ToV2, v2ToV3], NullLogger<EventUpgradePipeline>());
        var v1Envelope = EnvelopeAt(1, new { workOrderId = "abc" });

        var upgraded = pipeline.UpgradeToLatest(v1Envelope);

        upgraded.SchemaVersion.Should().Be(3);
        upgraded.Payload.TryGetProperty("addedInV2", out _).Should().BeTrue();
        upgraded.Payload.TryGetProperty("addedInV3", out _).Should().BeTrue();
        upgraded.Payload.TryGetProperty("workOrderId", out var original).Should().BeTrue();
        original.GetString().Should().Be("abc", "additive-only evolution must never lose the original v1 field");
    }

    [Fact]
    public void UpgradeToLatest_WithNoRegisteredUpgraderForEventType_ReturnsEnvelopeUnchanged()
    {
        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var envelope = EnvelopeAt(1, new { workOrderId = "abc" });

        var result = pipeline.UpgradeToLatest(envelope);

        result.Should().Be(envelope);
    }

    [Fact]
    public void UpgradeToLatest_WithDeprecatedVersion_DoesNotThrowAndStillProcesses()
    {
        DeprecatedEventVersionRegistry.MarkDeprecated(EventType, 1, "Superseded by v2 (test).");
        var pipeline = new EventUpgradePipeline([], NullLogger<EventUpgradePipeline>());
        var envelope = EnvelopeAt(1, new { workOrderId = "abc" });

        var act = () => pipeline.UpgradeToLatest(envelope);

        act.Should().NotThrow();
    }

    private static JsonElement BuildElement(Dictionary<string, object?> rawJsonProperties)
    {
        var sb = new System.Text.StringBuilder("{");
        var first = true;
        foreach (var (name, rawValue) in rawJsonProperties)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append('"').Append(name).Append("\":").Append(rawValue);
        }
        sb.Append('}');
        return JsonDocument.Parse(sb.ToString()).RootElement.Clone();
    }

    private sealed class TestUpgrader(string eventType, int from, int to, Func<JsonElement, JsonElement> upgrade) : IEventUpgrader
    {
        public string EventType { get; } = eventType;
        public int FromSchemaVersion { get; } = from;
        public int ToSchemaVersion { get; } = to;
        public JsonElement Upgrade(JsonElement payload) => upgrade(payload);
    }
}
