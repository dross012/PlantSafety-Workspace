// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           TestDataBuilder.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Fluent builder for deterministic, versioned integration test data.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.TestData;

/// <summary>
/// Fluent builder for constructing deterministic, minimal test data.
/// Uses fixed seeds to ensure reproducibility across CI environments.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>Returns a valid test TenantId.</summary>
    public static TenantId BuildTenantId() => TenantId.FromGuid(TestIds.TenantId);

    /// <summary>Returns a valid test UserId (CRUD authorship).</summary>
    public static UserId BuildUserId() => UserId.FromGuid(TestIds.ActorId);

    /// <summary>Returns a valid test CorrelationId.</summary>
    public static CorrelationId BuildCorrelationId() => CorrelationId.FromGuid(TestIds.CorrelationId);

    /// <summary>Returns a valid test CausationId.</summary>
    public static CausationId BuildCausationId() => CausationId.FromGuid(TestIds.CausationId);
}
