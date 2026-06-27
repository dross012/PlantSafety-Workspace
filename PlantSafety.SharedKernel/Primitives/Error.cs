// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Primitives
//  Layer:          SharedKernel
//  File:           Error.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Shallow namespace alias so callers in PlantSafety.SharedKernel.Primitives
//                  can use Error directly without importing the .Errors sub-namespace.
//                  The canonical definition lives in Primitives.Errors.Error.
//                  Additive-only evolution.
// ============================================================================

// Global using alias: expose the canonical type under the shallow namespace.
global using Error = PlantSafety.SharedKernel.Primitives.Errors.Error;
