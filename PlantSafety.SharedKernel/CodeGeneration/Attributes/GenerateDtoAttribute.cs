/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateDtoAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical DTOs following the Canonical DTO Rule
                        (immutable, normalized, validated, documented).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring a canonical DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateDtoAttribute : Attribute;
