// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Primitives.ValueTypes
//  Layer:          SharedKernel
//  File:           ActorKind.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Discriminator for the type of actor that performed a domain
//                  action. Used by PerformedByActor. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Discriminator for the kind of actor that performed a domain action.
/// </summary>
public enum ActorKind
{
    /// <summary>A human user authenticated via Azure AD.</summary>
    User = 1,

    /// <summary>An automated system process (e.g., OutboxProcessor, scheduler).</summary>
    System = 2,

    /// <summary>An AI/ML inference subsystem.</summary>
    AiAgent = 3,

    /// <summary>An external integration (e.g., a third-party API).</summary>
    External = 4
}
