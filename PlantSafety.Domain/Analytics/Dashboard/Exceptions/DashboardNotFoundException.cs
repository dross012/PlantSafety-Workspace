// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Exceptions;

/// <summary>Thrown when a <see cref="Dashboard"/> aggregate lookup finds no match.</summary>
public sealed class DashboardNotFoundException : DashboardDomainException
{
    /// <summary>Creates a not-found exception for the given Dashboard identity.</summary>
    public DashboardNotFoundException(DashboardId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Dashboard with id '{id}' was not found.",
            "Dashboard.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
