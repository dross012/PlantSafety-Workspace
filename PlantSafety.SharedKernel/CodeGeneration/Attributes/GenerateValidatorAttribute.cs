/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateValidatorAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical validators for commands and queries.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring a canonical validator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateValidatorAttribute : Attribute;
