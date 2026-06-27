/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               GenerateMappingAttribute.cs
    DESCRIPTION:        Attribute used by SafetyGenerator to generate canonical mapping profiles. Enforces the Canonical
                        Mapping Rule (explicit, deterministic, one-way, no reflection, no AutoMapper).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.CodeGeneration.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateMappingAttribute : Attribute
{
    public Type TargetType { get; }

    public GenerateMappingAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
