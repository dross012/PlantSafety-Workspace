// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           FakeCorrelationIdProvider.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Deterministic fake correlation and causation ID provider for unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

/// <summary>
/// Deterministic fake correlation context provider.
/// Uses fixed GUIDs so test assertions never depend on random values.
/// </summary>
public sealed class FakeCorrelationIdProvider
{
    /// <summary>Fixed test <see cref="CorrelationId"/>.</summary>
    public CorrelationId CorrelationId { get; } =
        CorrelationId.FromGuid(new Guid("00000003-0000-0000-0000-000000000003"));

    /// <summary>Fixed test <see cref="CausationId"/>.</summary>
    public CausationId CausationId { get; } =
        CausationId.FromGuid(new Guid("00000004-0000-0000-0000-000000000004"));

    /// <summary>Returns a new <see cref="CausationId"/> derived from the correlation for child operations.</summary>
    public CausationId NewCausationId() =>
        CausationId.FromGuid(Uuid7.New());
}
