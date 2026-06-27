// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Primitives.ValueTypes
//  Layer:          SharedKernel
//  File:           PerformedByActor.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical value type for domain actions. Distinguishes user
//                  actions (with UserId) from system actions.
//                  Rule: use PerformedByActor for domain actions; use UserId
//                  directly for CRUD authorship (CreatedBy, UpdatedBy).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Canonical value type identifying who performed a domain action.
/// <para>
/// RULE (Actor &amp; User Identity Semantics §1.8):
/// - Use <see cref="PerformedByActor"/> in domain events and command payloads.
/// - Use <see cref="UserId"/> directly on entities for CRUD authorship fields
///   (CreatedBy, UpdatedBy, DeletedBy).
/// </para>
/// </summary>
public readonly record struct PerformedByActor
{
    /// <summary>Kind of actor that performed the action.</summary>
    public ActorKind Kind { get; }

    /// <summary>
    /// Identity of the user actor. Present only when <see cref="Kind"/> is
    /// <see cref="ActorKind.User"/>; otherwise <c>null</c>.
    /// </summary>
    public UserId? UserId { get; }

    private PerformedByActor(ActorKind kind, UserId? userId) { Kind = kind; UserId = userId; }

    /// <summary>Creates an actor representing a human user.</summary>
    public static PerformedByActor User(UserId userId) => new(ActorKind.User, userId);

    /// <summary>Creates an actor representing an automated system process.</summary>
    public static PerformedByActor System() => new(ActorKind.System, null);

    /// <summary>Creates an actor representing an AI/ML inference subsystem.</summary>
    public static PerformedByActor AiAgent() => new(ActorKind.AiAgent, null);

    /// <summary>Creates an actor representing an external integration.</summary>
    public static PerformedByActor External() => new(ActorKind.External, null);

    public override string ToString()
        => Kind == ActorKind.User && UserId.HasValue
            ? $"User:{UserId.Value.Value:D}"
            : Kind.ToString();
}
