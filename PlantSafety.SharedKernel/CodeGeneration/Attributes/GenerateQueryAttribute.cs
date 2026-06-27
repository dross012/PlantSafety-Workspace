/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateQueryAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical query artifacts (handler, mapping, DTO,
                        registration). MUST be applied to record types representing queries.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as a canonical query requiring generation of handler, mapping, and registration artifacts.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateQueryAttribute : Attribute;
