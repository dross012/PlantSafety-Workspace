// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           IFinancialsDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Financials domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Events;

/// <summary>Marker interface for all Financials domain events.</summary>
public interface IFinancialsDomainEvent : IDomainEvent { }
