// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           UnitTestFixture.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Shared fixture providing pre-built fakes and builders for unit test classes.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Fixtures;

/// <summary>
/// Shared test fixture that pre-builds and exposes fakes used by multiple
/// test classes within the same xunit v3 collection.
/// All state is read-only after construction — safe for parallel test execution.
/// </summary>
public sealed class UnitTestFixture
{
    /// <summary>Deterministic fake clock frozen to 2026-06-17T12:00:00Z.</summary>
    public FakeClock Clock { get; } = new();

    /// <summary>Deterministic correlation context for the test session.</summary>
    public FakeCorrelationIdProvider Correlation { get; } = new();

    /// <summary>Fake domain event dispatcher for capturing raised events.</summary>
    public FakeDomainEventDispatcher EventDispatcher { get; } = new();
}
