// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           ValueObject.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Abstract base record for value objects. C# record semantics provide
//      structural equality for free. Concrete value objects should be
//      sealed records or readonly record structs depending on size.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Base;

/// <summary>
/// Abstract base for value objects. Inherit as a sealed record to obtain
/// structural equality, immutability, and with-expression support at no cost.
/// </summary>
public abstract record ValueObject;
