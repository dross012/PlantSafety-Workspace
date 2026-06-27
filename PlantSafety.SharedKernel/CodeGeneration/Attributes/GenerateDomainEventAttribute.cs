/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               GenerateDomainEventAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical domain event artifacts, including envelope,
                        metadata, versioning block, and registration.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

/// <summary>
/// Marks a type as a canonical domain event requiring generation of envelope, metadata, and versioning artifacts.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateDomainEventAttribute : Attribute;
