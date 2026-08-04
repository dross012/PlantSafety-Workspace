// ============================================================================
// File: LocalEmergencyCoordinationAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: Authorization policy name constants for the LocalEmergencyCoordination module.
//              Auto-discovered by AuthorizationPolicyExtensions via reflection (public static
//              string fields on any *AuthorizationPolicies class) -- no manual AddPolicy call
//              needed. Mirrors EmergencyManagementAuthorizationPolicies.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Authorization;

/// <summary>Authorization policy name constants for the LocalEmergencyCoordination module.</summary>
public static class LocalEmergencyCoordinationAuthorizationPolicies
{
    /// <summary>Read access to LocalEmergencyCoordination data. Granted to authorized roles.</summary>
    public const string Read = "RequireSafetyEventsLocalEmergencyCoordinationRead";

    /// <summary>Write access to LocalEmergencyCoordination data. Granted to administrative/emergency-response roles.</summary>
    public const string Write = "RequireSafetyEventsLocalEmergencyCoordinationWrite";
}
