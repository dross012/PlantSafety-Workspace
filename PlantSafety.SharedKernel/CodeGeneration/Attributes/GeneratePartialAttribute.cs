/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GeneratePartialAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to enforce partial class generation for extensibility and
                        additive-only evolution.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as requiring a generated partial class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratePartialAttribute : Attribute;
