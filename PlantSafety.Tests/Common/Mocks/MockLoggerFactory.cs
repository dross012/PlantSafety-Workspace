// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           MockLoggerFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    NSubstitute-based ILoggerFactory mock for unit tests that inject ILogger dependencies.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Mocks;

/// <summary>
/// Provides NSubstitute-based <see cref="ILoggerFactory"/> and typed logger mocks
/// for unit tests that need to inject <see cref="ILogger{T}"/> without output.
/// </summary>
public static class MockLoggerFactory
{
    /// <summary>Returns a typed <see cref="ILogger{T}"/> NSubstitute substitute.</summary>
    public static ILogger<T> Create<T>() => Substitute.For<ILogger<T>>();

    /// <summary>Returns an <see cref="ILoggerFactory"/> NSubstitute substitute.</summary>
    public static ILoggerFactory CreateFactory() => Substitute.For<ILoggerFactory>();
}
