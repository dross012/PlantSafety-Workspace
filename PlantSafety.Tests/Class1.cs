// ============================================================================
//  Project:        PlantSafety.Platform
//  Layer:          Tests (Unit)
//  File:           Class1.cs  (TestCollectionDefinitions)
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    xunit v3 collection definition for unit tests.
// ============================================================================
namespace PlantSafety.Tests;

/// <summary>
/// xunit v3 collection definition for unit tests.
/// Decorate test classes with <c>[Collection(nameof(UnitTestCollection))]</c>
/// to prevent parallel execution within the collection.
/// </summary>
[CollectionDefinition(nameof(UnitTestCollection))]
public sealed class UnitTestCollection { }
