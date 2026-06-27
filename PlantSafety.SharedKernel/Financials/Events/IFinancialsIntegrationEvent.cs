// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Financials
//  Layer:          SharedKernel
//  File:           IFinancialsIntegrationEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Financials integration events
//                  published to the Outbox. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Events;

namespace PlantSafety.SharedKernel.Financials.Events;

/// <summary>
/// Marker interface for Financials-area integration events.
/// All Financials integration events MUST implement this interface.
/// </summary>
public interface IFinancialsIntegrationEvent : IIntegrationEvent { }
