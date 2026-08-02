// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EmergencyManagement"/> aggregate.</summary>
public static class EmergencyManagementLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EmergencyManagementStatus current) => current == EmergencyManagementStatus.Created;

    /// <summary>Returns <see langword="true"/> when the emergency may be stabilized while in <paramref name="current"/>.</summary>
    public static bool CanStabilize(EmergencyManagementStatus current) => current == EmergencyManagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EmergencyManagementStatus current) => current == EmergencyManagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EmergencyManagementStatus current) => current == EmergencyManagementStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EmergencyManagementStatus current) => current == EmergencyManagementStatus.Closed;
}
