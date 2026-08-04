// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationDirection.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Direction of a logged communication with an external public-safety stakeholder.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;

/// <summary>Direction of a logged communication.</summary>
public enum CommunicationDirection
{
    /// <summary>Communication originated from PlantSafety to the external stakeholder.</summary>
    Outbound = 0,

    /// <summary>Communication originated from the external stakeholder to PlantSafety.</summary>
    Inbound = 1,
}
