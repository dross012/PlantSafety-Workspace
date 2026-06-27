// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           TestIds.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Fixed deterministic GUIDs shared across all integration tests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.TestConstants;

/// <summary>
/// Fixed, deterministic GUIDs used as test identifiers.
/// Using stable values ensures reproducible test scenarios and cross-test linkage.
/// </summary>
public static class TestIds
{
    /// <summary>Canonical test tenant identifier.</summary>
    public static readonly Guid TenantId = new("00000001-0000-0000-0000-000000000001");

    /// <summary>Canonical test actor (user) identifier.</summary>
    public static readonly Guid ActorId = new("00000002-0000-0000-0000-000000000002");

    /// <summary>Canonical test correlation identifier.</summary>
    public static readonly Guid CorrelationId = new("00000003-0000-0000-0000-000000000003");

    /// <summary>Canonical test causation identifier.</summary>
    public static readonly Guid CausationId = new("00000004-0000-0000-0000-000000000004");

    /// <summary>Canonical test site identifier.</summary>
    public static readonly Guid SiteId = new("00000005-0000-0000-0000-000000000005");

    /// <summary>Canonical test aggregate identifier (reused as a known existing record).</summary>
    public static readonly Guid AggregateId = new("00000010-0000-0000-0000-000000000010");
}
