/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateStrongIdAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical StrongId types, converters, and registration
                        artifacts. Enforces the StrongId Generation Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring a canonical StrongId.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateStrongIdAttribute : Attribute;
