// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           GlobalUsings.cs
//  Created:        2026-08-09
//  Description:    xunit.v3 introduced its own TestContext type, which
//                  collides with bUnit's TestContext (the base class every
//                  component test inherits from). This alias resolves the
//                  ambiguity repo-wide rather than qualifying it in every
//                  test file.
// ============================================================================
global using TestContext = Bunit.TestContext;
