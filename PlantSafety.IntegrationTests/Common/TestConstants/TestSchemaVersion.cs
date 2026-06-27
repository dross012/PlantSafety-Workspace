// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           TestSchemaVersion.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Canonical schema version constant used to assert event envelopes.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.TestConstants;

/// <summary>Schema version constants shared across all integration test assertions.</summary>
public static class TestSchemaVersion
{
    /// <summary>Current platform schema version — must match <c>InfrastructureConstants.EventSchemaVersion</c>.</summary>
    public const string Current = "2026.1";
}
