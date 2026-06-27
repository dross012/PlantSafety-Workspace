// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           AggregateBuilder.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Fluent builder helpers for constructing valid domain aggregates in unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Builders;

/// <summary>
/// Provides factory helpers for constructing domain aggregates with known,
/// deterministic values.  Used by all unit tests that need a pre-built aggregate.
/// </summary>
public static class AggregateBuilder
{
    /// <summary>Canonical frozen timestamp for all builders.</summary>
    public static readonly UtcTimestamp FrozenNow =
        UtcTimestamp.From(new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc));

    /// <summary>Canonical test user (CRUD authorship).</summary>
    public static readonly UserId DefaultUserId =
        UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002"));

    /// <summary>Canonical test correlation identifier.</summary>
    public static readonly CorrelationId DefaultCorrelationId =
        CorrelationId.FromGuid(new Guid("00000003-0000-0000-0000-000000000003"));

    /// <summary>Canonical test causation identifier.</summary>
    public static readonly CausationId DefaultCausationId =
        CausationId.FromGuid(new Guid("00000004-0000-0000-0000-000000000004"));

    /// <summary>Canonical <see cref="PerformedByActor"/> for all builders.</summary>
    public static readonly PerformedByActor DefaultActor =
        PerformedByActor.User(DefaultUserId);
}
