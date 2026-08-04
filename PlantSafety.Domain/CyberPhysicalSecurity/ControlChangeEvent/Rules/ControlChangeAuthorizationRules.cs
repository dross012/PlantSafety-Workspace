// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeAuthorizationRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless rules for classifying a control-system change's authorization
//                  status against a set of known approved change-management IDs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Rules;

/// <summary>Pure rules for classifying OT control-change authorization status.</summary>
public static class ControlChangeAuthorizationRules
{
    /// <summary>
    /// Classifies a control-change observation as Authorized, PendingReview, or Unauthorized based on
    /// whether an approved change-management reference exists and whether the change source is a
    /// known, attributable user identity.
    /// </summary>
    public static ControlChangeAuthorizationStatus Classify(bool hasApprovedChangeRecord, bool hasKnownChangeSource)
    {
        if (hasApprovedChangeRecord)
            return ControlChangeAuthorizationStatus.Authorized;

        return hasKnownChangeSource
            ? ControlChangeAuthorizationStatus.PendingReview
            : ControlChangeAuthorizationStatus.Unauthorized;
    }
}
