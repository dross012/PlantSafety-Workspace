// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Fixtures
//  Layer:          IntegrationTests
//  File:           SharedFixtures.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Shared xunit fixtures and collection definitions for cross-module integration tests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Fixtures;

/// <summary>
/// Marker class for fixtures and collection definitions that span multiple test areas.
/// All module-specific test classes reference <see cref="IntegrationTestCollection"/>
/// from the Setup namespace to share the single <see cref="IntegrationTestWebAppFactory"/> instance.
/// </summary>
public static class SharedFixtures
{
    // Intentionally empty — collection fixture registration is in Setup\InitTestHost.cs.
}
