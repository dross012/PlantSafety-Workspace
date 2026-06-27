/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateCommandAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical command artifacts (handler, validator,
                        mapping, DTO, registration). MUST be applied to record types representing commands.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as a canonical command requiring generation of handler, validator, mapping, and registration artifacts.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateCommandAttribute : Attribute;
