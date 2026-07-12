// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EmergencyManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Policies;

/// <summary>Determines whether a <see cref="EmergencyManagement"/> aggregate is eligible for archival.</summary>
public static class EmergencyManagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the EmergencyManagement may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement aggregate)
        => aggregate.Status == EmergencyManagementStatus.Closed;
}
