// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           StubValueObjects.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pre-built, deterministic value object instances for use across unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Stubs;

/// <summary>
/// Pre-built, deterministic value object instances.
/// These represent the canonical "happy path" test values — valid inputs
/// that pass all domain invariants without any setup ceremony.
/// </summary>
public static class StubValueObjects
{
    /// <summary>Canonical test <see cref="TenantId"/>.</summary>
    public static TenantId TenantId { get; } =
        TenantId.FromGuid(new Guid("00000001-0000-0000-0000-000000000001"));

    /// <summary>Canonical test <see cref="UserId"/>.</summary>
    public static UserId UserId { get; } =
        UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002"));

    /// <summary>Canonical test <see cref="CorrelationId"/>.</summary>
    public static CorrelationId CorrelationId { get; } =
        CorrelationId.FromGuid(new Guid("00000003-0000-0000-0000-000000000003"));

    /// <summary>Canonical test <see cref="CausationId"/>.</summary>
    public static CausationId CausationId { get; } =
        CausationId.FromGuid(new Guid("00000004-0000-0000-0000-000000000004"));

    /// <summary>Canonical frozen <see cref="UtcTimestamp"/>.</summary>
    public static UtcTimestamp FrozenNow { get; } =
        UtcTimestamp.From(new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc));

    /// <summary>Canonical <see cref="PerformedByActor"/> (User).</summary>
    public static PerformedByActor Actor { get; } =
        PerformedByActor.User(UserId);
}
