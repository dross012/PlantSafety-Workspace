// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Orchestration
//  Layer:          Abstractions
//  File:           OrchestrationEventCategory.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic classification of a real event's lifecycle
//                  phase, derived purely from its EventType string suffix
//                  (e.g. "Accidents.AccidentCreated" ends with "Created";
//                  "Risk.RiskArchived" ends with "Archived") -- every
//                  EventType this layer actually subscribes to today
//                  (Accidents.AccidentCreated/AccidentArchived,
//                  Risk.RiskCreated/RiskArchived) follows this convention.
//                  Not a hazard-type classification (see EventDefinition.cs's
//                  ARCHITECTURAL SCOPE note for why this layer does not
//                  attempt that) -- purely "is this a new record or a closed
//                  one".
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

/// <summary>The lifecycle phase a real event's EventType string indicates.</summary>
public enum OrchestrationEventCategory
{
    /// <summary>EventType ends with "Created" -- a new record was opened.</summary>
    Created,

    /// <summary>EventType ends with "Archived" -- a closed record was archived.</summary>
    Archived,

    /// <summary>Neither suffix matched -- an event type not yet classified by this rule.</summary>
    Other,
}

/// <summary>Derives <see cref="OrchestrationEventCategory"/> from an EventType string.</summary>
public static class OrchestrationEventCategoryCatalog
{
    /// <summary>Classifies the given EventType by its suffix.</summary>
    public static OrchestrationEventCategory ForEventType(string eventType) => eventType switch
    {
        _ when eventType.EndsWith("Created", StringComparison.Ordinal) => OrchestrationEventCategory.Created,
        _ when eventType.EndsWith("Archived", StringComparison.Ordinal) => OrchestrationEventCategory.Archived,
        _ => OrchestrationEventCategory.Other,
    };
}
