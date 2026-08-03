// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyInsightsDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all GlobalSafetyInsights domain events, including the
//                  Phase 8 §4 internal cross-site event-mesh extensions (GlobalSafetyTelemetryEvent,
//                  CrossSiteActionEvent, GlobalSafetyKpiSnapshotEvent, GlobalSafetyAnomalyEvent).
//                  Modeled as real Domain Events (not a separate SharedKernel contract) per the
//                  existing internal Outbox -> MassTransit domain-event convention (Phase 5 Cross-
//                  Module Orchestration), since every one of these is a purely internal, intra-
//                  platform cross-site signal -- distinct from Phase 6's external-system Enterprise
//                  Event Mesh (PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh). Additive-
//                  only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Marker interface for all GlobalSafetyInsights domain events.</summary>
public interface IGlobalSafetyInsightsDomainEvent : IDomainEvent { }
