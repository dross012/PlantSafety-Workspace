/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateVersionHistoryAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical version history blocks for domain events,
                        DTOs, and other versioned artifacts. Enforces the Canonical Versioning Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring a canonical version history block.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateVersionHistoryAttribute : Attribute;
