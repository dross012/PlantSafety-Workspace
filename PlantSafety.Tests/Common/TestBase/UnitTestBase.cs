// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           UnitTestBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base class for all unit tests. Provides deterministic IDs, frozen clock, and correlation context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

/// <summary>
/// Abstract base for all PlantSafety unit tests.
/// Provides deterministic identifiers, a frozen clock, and canonical context
/// values so every test starts from the same well-known state.
/// All derived tests MUST run in under 50 ms (per architectural rule §4.4).
/// </summary>
public abstract class UnitTestBase
{
    // ---- Frozen deterministic clock ----------------------------------------

    /// <summary>Frozen UTC instant used by all unit tests.</summary>
    protected static readonly DateTime FixedUtcNow = new(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Frozen <see cref="UtcTimestamp"/> for aggregate creation assertions.</summary>
    protected static readonly UtcTimestamp FixedTimestamp = UtcTimestamp.From(FixedUtcNow);

    // ---- Canonical test identifiers ----------------------------------------

    /// <summary>Deterministic test tenant identifier.</summary>
    protected static readonly TenantId TestTenantId = TenantId.FromGuid(new Guid("00000001-0000-0000-0000-000000000001"));

    /// <summary>Deterministic test user identifier (CRUD authorship).</summary>
    protected static readonly UserId TestUserId = UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002"));

    /// <summary>Deterministic test correlation identifier.</summary>
    protected static readonly CorrelationId TestCorrelationId = CorrelationId.FromGuid(new Guid("00000003-0000-0000-0000-000000000003"));

    /// <summary>Deterministic test causation identifier.</summary>
    protected static readonly CausationId TestCausationId = CausationId.FromGuid(new Guid("00000004-0000-0000-0000-000000000004"));

    /// <summary>Canonical <see cref="PerformedByActor"/> for domain action tests.</summary>
    protected static readonly PerformedByActor TestActor = PerformedByActor.User(TestUserId);

    // ---- Logger factory (null — no side-effects) ---------------------------

    /// <summary>NullLogger factory for injecting ILogger without output.</summary>
    protected static ILogger<T> NullLogger<T>() => Microsoft.Extensions.Logging.Abstractions.NullLogger<T>.Instance;
}
