// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the ContractorVisitor aggregate.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Rules;

/// <summary>Defines permitted lifecycle transitions for the ContractorVisitor aggregate.</summary>
public static class ContractorVisitorLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(ContractorVisitorStatus current) => current == ContractorVisitorStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(ContractorVisitorStatus current) => current == ContractorVisitorStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(ContractorVisitorStatus current) => current == ContractorVisitorStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ContractorVisitorStatus current) => current == ContractorVisitorStatus.Closed;
}
