// ============================================================================
//  Project:        Shared.EventBus.Tests
//  File:           IntegrationEventContractTests.cs
//  Created:        2026-07-27
//  Description:    Cross-module contract tests: every DTO in Shared.DomainEvents.Abstractions
//                  must expose a stable "<Module>.<EventName>" EventType, a positive
//                  SchemaVersion, and must round-trip through EventEnvelope JSON
//                  serialization without data loss. Guards against accidental breaking
//                  changes to the wire contract other modules (e.g. Reporting) depend on.
// ============================================================================
using Shared.DomainEvents.Abstractions;
using Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.CAPA;
using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.RiskManagement;
using Shared.EventBus.Abstractions.Contracts;

using Xunit;

namespace Shared.EventBus.Tests.Contracts;

public sealed class IntegrationEventContractTests
{
    [Theory]
    [InlineData(typeof(IncidentManagementCreatedIntegrationEvent), "Incidents.IncidentManagementCreated")]
    [InlineData(typeof(IncidentManagementUpdatedIntegrationEvent), "Incidents.IncidentManagementUpdated")]
    [InlineData(typeof(IncidentManagementArchivedIntegrationEvent), "Incidents.IncidentManagementArchived")]
    [InlineData(typeof(CorrectiveActionCreatedIntegrationEvent), "CAPA.CorrectiveActionCreated")]
    [InlineData(typeof(CorrectiveActionStartedIntegrationEvent), "CAPA.CorrectiveActionStarted")]
    [InlineData(typeof(CorrectiveActionCompletedIntegrationEvent), "CAPA.CorrectiveActionCompleted")]
    [InlineData(typeof(CorrectiveActionClosedIntegrationEvent), "CAPA.CorrectiveActionClosed")]
    [InlineData(typeof(CorrectiveActionMarkedOverdueIntegrationEvent), "CAPA.CorrectiveActionMarkedOverdue")]
    [InlineData(typeof(AccidentCreatedIntegrationEvent), "Accidents.AccidentCreated")]
    [InlineData(typeof(AccidentArchivedIntegrationEvent), "Accidents.AccidentArchived")]
    [InlineData(typeof(RiskCreatedIntegrationEvent), "Risk.RiskCreated")]
    [InlineData(typeof(RiskArchivedIntegrationEvent), "Risk.RiskArchived")]
    public void EventType_MatchesModuleDotEventNameConvention(Type dtoType, string expectedEventType)
    {
        var eventTypeProperty = dtoType.GetProperty(nameof(IIntegrationEventContract.EventType))!;
        var actual = (string)eventTypeProperty.GetValue(null)!;

        Assert.Equal(expectedEventType, actual);
        // patterns.md §4: "<Module>.<AggregateAction>" -- exactly one dot, PascalCase segments.
        Assert.Matches(@"^[A-Z][A-Za-z]*\.[A-Z][A-Za-z]*$", actual);
    }

    [Theory]
    [InlineData(typeof(IncidentManagementCreatedIntegrationEvent))]
    [InlineData(typeof(CorrectiveActionCreatedIntegrationEvent))]
    [InlineData(typeof(AccidentCreatedIntegrationEvent))]
    [InlineData(typeof(RiskCreatedIntegrationEvent))]
    public void SchemaVersion_IsPositive(Type dtoType)
    {
        var schemaVersionProperty = dtoType.GetProperty(nameof(IIntegrationEventContract.SchemaVersion))!;
        var actual = (int)schemaVersionProperty.GetValue(null)!;

        Assert.True(actual >= 1);
    }

    [Fact]
    public void EventEnvelope_RoundTripsIncidentManagementCreatedPayload()
    {
        var payload = new IncidentManagementCreatedIntegrationEvent(
            IncidentManagementId: Guid.NewGuid(),
            TenantId: "tenant-acme",
            CreatedAtUtc: DateTime.UtcNow,
            ActorId: Guid.NewGuid(),
            ActorKind: "User",
            CorrelationId: Guid.NewGuid(),
            CausationId: Guid.NewGuid());

        var envelope = EventEnvelope.Create(
            payload,
            tenantId: payload.TenantId,
            correlationId: payload.CorrelationId,
            causationId: payload.CausationId,
            occurredAtUtc: payload.CreatedAtUtc,
            sourceModule: "PlantSafety",
            eventType: IncidentManagementCreatedIntegrationEvent.EventType,
            schemaVersion: IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        Assert.Equal("Incidents.IncidentManagementCreated", envelope.EventType);
        Assert.Equal(1, envelope.SchemaVersion);
        Assert.Equal(nameof(IncidentManagementCreatedIntegrationEvent), envelope.PayloadType);

        var roundTripped = envelope.DeserializePayload<IncidentManagementCreatedIntegrationEvent>();

        Assert.Equal(payload, roundTripped);
    }

    [Fact]
    public void EventEnvelope_RoundTripsCorrectiveActionClosedPayload()
    {
        var payload = new CorrectiveActionClosedIntegrationEvent(
            CorrectiveActionId: Guid.NewGuid(),
            TenantId: "tenant-acme",
            ClosedAtUtc: DateTime.UtcNow,
            ActorId: null,
            ActorKind: "System",
            CorrelationId: Guid.NewGuid(),
            CausationId: Guid.NewGuid());

        var envelope = EventEnvelope.Create(
            payload, payload.TenantId, payload.CorrelationId, payload.CausationId, payload.ClosedAtUtc,
            "PlantSafety", CorrectiveActionClosedIntegrationEvent.EventType, CorrectiveActionClosedIntegrationEvent.SchemaVersion);

        var roundTripped = envelope.DeserializePayload<CorrectiveActionClosedIntegrationEvent>();

        Assert.Equal(payload, roundTripped);
        Assert.Null(roundTripped.ActorId);
    }
}
