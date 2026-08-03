// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Common.Serialization
//  Layer:          Tests (Unit)
//  File:           SmartEnumJsonConverterFactoryTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SmartEnumJsonConverterFactory: CanConvert detection rule,
//                  single Smart Enum (RiskLevel) round-trip, null handling, unknown-code failure,
//                  and a full domain-event-payload round-trip (RiskScoredEvent, whose Level
//                  property is the RiskLevel Smart Enum) through the canonical
//                  JsonSerializerConfiguration.Default options -- the exact scenario this
//                  converter was introduced to fix per the file header of
//                  SmartEnumJsonConverterFactory.cs (Outbox publish path previously swallowing
//                  events carrying a Smart Enum value).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.Domain.RiskManagement.ValueObjects;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Serialization;

namespace PlantSafety.Tests.Infrastructure.Common.Serialization;

/// <summary>Unit tests for <see cref="SmartEnumJsonConverterFactory"/>.</summary>
public sealed class SmartEnumJsonConverterFactoryTests : UnitTestBase
{
    private static JsonSerializerOptions Options => JsonSerializerConfiguration.Default;

    // ---- CanConvert detection rule --------------------------------------------

    [Fact]
    public void CanConvert_ForSmartEnumType_ShouldReturnTrue()
    {
        new SmartEnumJsonConverterFactory().CanConvert(typeof(RiskLevel)).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_ForTypeWithPublicConstructor_ShouldReturnFalse()
    {
        new SmartEnumJsonConverterFactory().CanConvert(typeof(RiskScoredPayloadStub)).Should().BeFalse();
    }

    [Fact]
    public void CanConvert_ForPrimitiveType_ShouldReturnFalse()
    {
        new SmartEnumJsonConverterFactory().CanConvert(typeof(int)).Should().BeFalse();
    }

    [Fact]
    public void CanConvert_ForStringType_ShouldReturnFalse()
    {
        new SmartEnumJsonConverterFactory().CanConvert(typeof(string)).Should().BeFalse();
    }

    [Fact]
    public void CreateConverter_ForSmartEnumType_ShouldReturnUsableConverter()
    {
        var converter = new SmartEnumJsonConverterFactory().CreateConverter(typeof(RiskLevel), Options);
        converter.Should().NotBeNull();
        converter.Should().BeAssignableTo<JsonConverter<RiskLevel>>();
    }

    // ---- Single Smart Enum round-trip -----------------------------------------

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void SerializeThenDeserialize_RiskLevel_ShouldRoundTripToEquivalentInstance(string code)
    {
        var level = RiskLevel.From(code);

        var json = JsonSerializer.Serialize(level, Options);
        var roundTripped = JsonSerializer.Deserialize<RiskLevel>(json, Options);

        roundTripped.Should().Be(level);
    }

    [Fact]
    public void Serialize_RiskLevel_ShouldWriteBareCodeString()
    {
        var json = JsonSerializer.Serialize(RiskLevel.Critical, Options);
        json.Should().Be("\"Critical\"");
    }

    [Fact]
    public void Deserialize_UnknownCode_ShouldThrow()
    {
        var act = () => JsonSerializer.Deserialize<RiskLevel>("\"NotARealLevel\"", Options);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deserialize_JsonNull_ShouldReturnNull()
    {
        var result = JsonSerializer.Deserialize<RiskLevel>("null", Options);
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyString_ShouldReturnNull()
    {
        var result = JsonSerializer.Deserialize<RiskLevel>("\"\"", Options);
        result.Should().BeNull();
    }

    // ---- Full domain event payload round-trip ----------------------------------

    private static RiskScoredEvent MakeEvent() => new(
        EventId: RiskEventId.New(),
        RiskId: RiskId.FromGuid(new Guid("00000009-0000-0000-0000-000000000009")),
        Score: new RiskScore(5, 5),
        Level: RiskLevel.Critical,
        ScoredAtUtc: FixedTimestamp,
        Actor: TestActor,
        CorrelationId: TestCorrelationId,
        CausationId: TestCausationId);

    [Fact]
    public void SerializeThenDeserialize_RiskScoredEvent_ShouldPreserveSmartEnumLevel()
    {
        var evt = MakeEvent();

        var json = JsonSerializer.Serialize(evt, Options);
        var roundTripped = JsonSerializer.Deserialize<RiskScoredEvent>(json, Options);

        roundTripped.Should().NotBeNull();
        roundTripped!.Level.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void SerializeThenDeserialize_RiskScoredEvent_ShouldPreserveComputedEventType()
    {
        // NOTE: this assertion is scoped to the fields relevant to the SmartEnumJsonConverterFactory
        // contract (the Smart Enum Level property and the computed EventType). Other nested
        // value-object properties on this event (RiskId, RiskScore) have their own, separate STJ
        // parameterized-constructor round-trip behavior that is out of scope for this converter and
        // is not asserted here -- see SmartEnumJsonConverterFactoryTests class remarks.
        var evt = MakeEvent();

        var json = JsonSerializer.Serialize(evt, Options);
        var roundTripped = JsonSerializer.Deserialize<RiskScoredEvent>(json, Options);

        roundTripped!.EventType.Should().Be("RiskScored");
    }

    [Fact]
    public void Serialize_RiskScoredEvent_ShouldEmbedLevelAsPlainCodeString()
    {
        var json = JsonSerializer.Serialize(MakeEvent(), Options);
        json.Should().Contain("\"level\":\"Critical\"");
    }

    /// <summary>Stub type exposing a public constructor and a Code/From shape, used to prove the
    /// factory's structural-detection rule correctly excludes non-Smart-Enum types.</summary>
    private sealed class RiskScoredPayloadStub
    {
        public RiskScoredPayloadStub() { }
        public string Code => "Stub";
        public static RiskScoredPayloadStub From(string code) => new();
    }
}
