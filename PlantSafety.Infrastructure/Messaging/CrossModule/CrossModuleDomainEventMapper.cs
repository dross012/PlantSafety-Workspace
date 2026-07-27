// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           CrossModuleDomainEventMapper.cs
//  Created:        2026-07-27
//  Description:    Maps the curated set of PlantSafety.Domain events that have a registered
//                  Shared.DomainEvents.Abstractions DTO onto an EventEnvelope. Scope is
//                  deliberately limited to the events named in the cross-module eventing
//                  checklist (IncidentManagement Created/Updated/Archived; CAPA
//                  CorrectiveAction Created/Started/Completed/Closed/MarkedOverdue;
//                  AccidentManagement Created/Archived; RiskManagement Created/Archived) --
//                  PlantSafety.Domain contains ~200 domain events across ~80 aggregates in
//                  total (see docs/architecture/eventbus-architecture.md §"Domain Event
//                  Inventory" for the full list); only this curated subset has a published,
//                  versioned cross-module contract today. Extending coverage to another
//                  domain event only requires: (1) a new DTO in
//                  Shared.DomainEvents.Abstractions, (2) a new case arm below.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Events;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

using Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.CAPA;
using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.RiskManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <summary>Maps supported <see cref="IDomainEvent"/> instances to a cross-module <see cref="EventEnvelope"/>.</summary>
public static class CrossModuleDomainEventMapper
{
    private const string SourceModule = "PlantSafety";

    /// <summary>
    /// Attempts to build an <see cref="EventEnvelope"/> for <paramref name="domainEvent"/>.
    /// Returns <see langword="false"/> (no envelope) for any domain event without a registered
    /// cross-module DTO -- this is the expected, common case, not an error.
    /// </summary>
    public static bool TryMap(IDomainEvent domainEvent, string tenantId, out EventEnvelope? envelope)
    {
        envelope = domainEvent switch
        {
            IncidentManagementCreatedEvent e => EventEnvelope.Create(
                new IncidentManagementCreatedIntegrationEvent(
                    e.IncidentManagementId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            IncidentManagementUpdatedEvent e => EventEnvelope.Create(
                new IncidentManagementUpdatedIntegrationEvent(
                    e.IncidentManagementId.Value, tenantId, e.UpdatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                IncidentManagementUpdatedIntegrationEvent.EventType, IncidentManagementUpdatedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            IncidentManagementArchivedEvent e => EventEnvelope.Create(
                new IncidentManagementArchivedIntegrationEvent(
                    e.IncidentManagementId.Value, tenantId, e.ArchivedAtUtc.Value, e.DeletionReason,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                IncidentManagementArchivedIntegrationEvent.EventType, IncidentManagementArchivedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            CorrectiveActionCreatedEvent e => EventEnvelope.Create(
                new CorrectiveActionCreatedIntegrationEvent(
                    e.CorrectiveActionId.Value, tenantId, e.SourceEventId, e.SourceEventType, e.AssignedToWorkerId.Value,
                    e.DueAtUtc.Value, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CorrectiveActionCreatedIntegrationEvent.EventType, CorrectiveActionCreatedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            CorrectiveActionStartedEvent e => EventEnvelope.Create(
                new CorrectiveActionStartedIntegrationEvent(
                    e.CorrectiveActionId.Value, tenantId, e.StartedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CorrectiveActionStartedIntegrationEvent.EventType, CorrectiveActionStartedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            CorrectiveActionCompletedEvent e => EventEnvelope.Create(
                new CorrectiveActionCompletedIntegrationEvent(
                    e.CorrectiveActionId.Value, tenantId, e.CompletedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CorrectiveActionCompletedIntegrationEvent.EventType, CorrectiveActionCompletedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            CorrectiveActionClosedEvent e => EventEnvelope.Create(
                new CorrectiveActionClosedIntegrationEvent(
                    e.CorrectiveActionId.Value, tenantId, e.ClosedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CorrectiveActionClosedIntegrationEvent.EventType, CorrectiveActionClosedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            CorrectiveActionMarkedOverdueEvent e => EventEnvelope.Create(
                new CorrectiveActionMarkedOverdueIntegrationEvent(
                    e.CorrectiveActionId.Value, tenantId, e.MarkedOverdueAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CorrectiveActionMarkedOverdueIntegrationEvent.EventType, CorrectiveActionMarkedOverdueIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            AccidentCreatedEvent e => EventEnvelope.Create(
                new AccidentCreatedIntegrationEvent(
                    e.AccidentId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentCreatedIntegrationEvent.EventType, AccidentCreatedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            AccidentArchivedEvent e => EventEnvelope.Create(
                new AccidentArchivedIntegrationEvent(
                    e.AccidentId.Value, tenantId, e.ArchivedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentArchivedIntegrationEvent.EventType, AccidentArchivedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            RiskCreatedEvent e => EventEnvelope.Create(
                new RiskCreatedIntegrationEvent(
                    e.RiskId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                RiskCreatedIntegrationEvent.EventType, RiskCreatedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            RiskArchivedEvent e => EventEnvelope.Create(
                new RiskArchivedIntegrationEvent(
                    e.RiskId.Value, tenantId, e.ArchivedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                RiskArchivedIntegrationEvent.EventType, RiskArchivedIntegrationEvent.SchemaVersion,
                e.EventId.Value),

            _ => null,
        };

        return envelope is not null;
    }
}
