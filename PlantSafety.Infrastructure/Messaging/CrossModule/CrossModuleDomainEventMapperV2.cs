// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.CrossModule
//  Layer:          Infrastructure
//  File:           CrossModuleDomainEventMapperV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 (Path B: Transformative Modernization). Maps a
//                  curated set of PlantSafety.Domain events onto an
//                  EventEnvelopeV2, enriched with FacilityId/IndustryName/
//                  Location/HazardType via IFacilityInferenceResolver/
//                  HazardInferenceRules. A NEW, parallel mapper alongside
//                  CrossModuleDomainEventMapper (V1) -- not a modification
//                  of it. Every domain event handled here is read-only
//                  input: no PlantSafety.Domain aggregate, event, or command
//                  is touched by this file.
//
//                  Scope: the 6 AccidentManagement events named in Phase
//                  B.3, plus one representative event each for
//                  IncidentManagement (the real "a safety event occurred"
//                  fact -- see IncidentManagementCreatedIntegrationEventV2.cs's
//                  header for why), CrisisManagement, and EmergencyManagement.
//                  CrisisManagement/EmergencyManagement had ZERO cross-module
//                  contract before this phase (verified: no
//                  ICrossModuleEventRelay usage anywhere under
//                  PlantSafety.Infrastructure.SafetyEvents.{CrisisManagement,
//                  EmergencyManagement} prior to this file).
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

using Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.CrisisManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.EmergencyManagement;
using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Infrastructure.Messaging.CrossModule;

/// <summary>Maps a curated subset of <see cref="IDomainEvent"/> instances to a V2 <see cref="EventEnvelopeV2"/>, enriched with facility/hazard metadata.</summary>
public static class CrossModuleDomainEventMapperV2
{
    private const string SourceModule = "PlantSafety";

    /// <summary>
    /// Attempts to build an <see cref="EventEnvelopeV2"/> for <paramref name="domainEvent"/>.
    /// HazardType is inferred synchronously here (via HazardInferenceRules, from EventType plus
    /// whatever free text the domain event actually carries -- e.g. CrisisManagementDeclaredEvent's
    /// Title). FacilityId/IndustryName/Location are left null: resolving those is I/O
    /// (IFacilityInferenceResolver), so callers apply FacilityInferenceRules.EnrichWithFacilityAsync
    /// separately (see CrossModuleEventRelayV2.FlushAsync), mirroring
    /// CrossModuleDomainEventMapper's own synchronous TryMap shape. Returns
    /// <see langword="false"/> for any domain event without a registered V2 DTO -- the expected,
    /// common case, not an error.
    /// </summary>
    public static bool TryMap(IDomainEvent domainEvent, string tenantId, out EventEnvelopeV2? envelope)
    {
        envelope = domainEvent switch
        {
            AccidentCreatedEvent e => EventEnvelopeV2.Create(
                new AccidentCreatedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentCreatedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentCreatedIntegrationEventV2.EventType, AccidentCreatedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentCreatedIntegrationEventV2.EventType)),

            AccidentArchivedEvent e => EventEnvelopeV2.Create(
                new AccidentArchivedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.ArchivedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentArchivedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentArchivedIntegrationEventV2.EventType, AccidentArchivedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentArchivedIntegrationEventV2.EventType)),

            AccidentInvestigationClosedEvent e => EventEnvelopeV2.Create(
                new AccidentInvestigationClosedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.ClosedBy.Value, e.ClosedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentInvestigationClosedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentInvestigationClosedIntegrationEventV2.EventType, AccidentInvestigationClosedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentInvestigationClosedIntegrationEventV2.EventType)),

            AccidentRootCauseRecordedEvent e => EventEnvelopeV2.Create(
                new AccidentRootCauseRecordedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.RootCause.ToString(), e.RecordedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentRootCauseRecordedIntegrationEventV2.EventType, e.RootCause.ToString()),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentRootCauseRecordedIntegrationEventV2.EventType, AccidentRootCauseRecordedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentRootCauseRecordedIntegrationEventV2.EventType, e.RootCause.ToString())),

            AccidentCorrectiveActionCreatedEvent e => EventEnvelopeV2.Create(
                new AccidentCorrectiveActionCreatedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.CorrectiveActionId.Value, e.Category.ToString(), e.ResponsiblePersonId.Value,
                    e.DueAtUtc.Value, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentCorrectiveActionCreatedIntegrationEventV2.EventType, e.Category.ToString()),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentCorrectiveActionCreatedIntegrationEventV2.EventType, AccidentCorrectiveActionCreatedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentCorrectiveActionCreatedIntegrationEventV2.EventType, e.Category.ToString())),

            AccidentRegulatoryFilingCompletedEvent e => EventEnvelopeV2.Create(
                new AccidentRegulatoryFilingCompletedIntegrationEventV2(
                    e.AccidentId.Value, tenantId, e.FiledBy.Value, e.RegulatoryAgency, e.ReferenceNumber, e.FiledAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentRegulatoryFilingCompletedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                AccidentRegulatoryFilingCompletedIntegrationEventV2.EventType, AccidentRegulatoryFilingCompletedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromAccident(AccidentRegulatoryFilingCompletedIntegrationEventV2.EventType)),

            IncidentManagementCreatedEvent e => EventEnvelopeV2.Create(
                new IncidentManagementCreatedIntegrationEventV2(
                    e.IncidentManagementId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromSafetyEvent(IncidentManagementCreatedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                IncidentManagementCreatedIntegrationEventV2.EventType, IncidentManagementCreatedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromSafetyEvent(IncidentManagementCreatedIntegrationEventV2.EventType)),

            CrisisManagementDeclaredEvent e => EventEnvelopeV2.Create(
                new CrisisManagementDeclaredIntegrationEventV2(
                    e.CrisisManagementId.Value, tenantId, e.SiteId, e.Title, e.Severity, e.DeclaredAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromCrisisEvent(CrisisManagementDeclaredIntegrationEventV2.EventType, e.Title),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                CrisisManagementDeclaredIntegrationEventV2.EventType, CrisisManagementDeclaredIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromCrisisEvent(CrisisManagementDeclaredIntegrationEventV2.EventType, e.Title)),

            EmergencyManagementCreatedEvent e => EventEnvelopeV2.Create(
                new EmergencyManagementCreatedIntegrationEventV2(
                    e.EmergencyManagementId.Value, tenantId, e.CreatedAtUtc.Value,
                    e.Actor.UserId?.Value, e.Actor.Kind.ToString(), e.CorrelationId.Value, e.CausationId.Value,
                    FacilityId: null, IndustryName: null, Location: null,
                    HazardType: HazardInferenceRules.InferHazardTypeFromEmergencyEvent(EmergencyManagementCreatedIntegrationEventV2.EventType),
                    TimestampUtc: e.OccurredAtUtc.Value),
                tenantId, e.CorrelationId.Value, e.CausationId.Value, e.OccurredAtUtc.Value, SourceModule,
                EmergencyManagementCreatedIntegrationEventV2.EventType, EmergencyManagementCreatedIntegrationEventV2.SchemaVersion,
                e.EventId.Value,
                hazardType: HazardInferenceRules.InferHazardTypeFromEmergencyEvent(EmergencyManagementCreatedIntegrationEventV2.EventType)),

            _ => null,
        };

        return envelope is not null;
    }
}
