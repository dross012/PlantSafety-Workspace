/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateRegistrationAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical registration entries for commands, queries,
                        domain events, and StrongIds.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring canonical registration artifacts.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateRegistrationAttribute : Attribute;
