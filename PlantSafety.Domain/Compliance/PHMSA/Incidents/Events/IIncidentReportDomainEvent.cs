// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IIncidentReportDomainEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all IncidentReport domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;

/// <summary>Marker interface for all IncidentReport domain events.</summary>
public interface IIncidentReportDomainEvent : IDomainEvent { }
