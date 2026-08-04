// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationMedium.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure classification of the medium used for a logged communication. No
//                  business rules or workflow metadata attach to this vocabulary, so a simple
//                  enum is used per the Enums Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;

/// <summary>Medium used for a logged communication.</summary>
public enum CommunicationMedium
{
    /// <summary>Telephone call.</summary>
    Phone = 0,

    /// <summary>Email message.</summary>
    Email = 1,

    /// <summary>Facsimile transmission.</summary>
    Fax = 2,

    /// <summary>SMS/text message.</summary>
    Sms = 3,

    /// <summary>Two-way radio transmission.</summary>
    RadioDispatch = 4,

    /// <summary>Computer-Aided Dispatch (CAD) system integration event.</summary>
    CadIntegration = 5,

    /// <summary>Face-to-face/in-person communication.</summary>
    InPerson = 6,

    /// <summary>Public alert/warning system transmission.</summary>
    PublicAlertSystem = 7,
}
