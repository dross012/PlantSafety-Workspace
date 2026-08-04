// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for all CyberPhysicalIncident domain rule violations.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Exceptions;

/// <summary>Base exception for CyberPhysicalIncident domain rule violations.</summary>
public class CyberPhysicalIncidentDomainException : Exception
{
    /// <summary>Creates a new <see cref="CyberPhysicalIncidentDomainException"/>.</summary>
    public CyberPhysicalIncidentDomainException(string message) : base(message) { }
}

/// <summary>Raised when a CyberPhysicalIncident cannot be located by identity.</summary>
public sealed class CyberPhysicalIncidentNotFoundException : CyberPhysicalIncidentDomainException
{
    /// <summary>Creates a new <see cref="CyberPhysicalIncidentNotFoundException"/>.</summary>
    public CyberPhysicalIncidentNotFoundException(Guid cyberPhysicalIncidentId)
        : base($"CyberPhysicalIncident '{cyberPhysicalIncidentId}' was not found.") { }
}

/// <summary>Raised when a CyberPhysicalIncident lifecycle transition is attempted from an invalid state.</summary>
public sealed class CyberPhysicalIncidentInvalidStatusTransitionException : CyberPhysicalIncidentDomainException
{
    /// <summary>Creates a new <see cref="CyberPhysicalIncidentInvalidStatusTransitionException"/>.</summary>
    public CyberPhysicalIncidentInvalidStatusTransitionException(CyberPhysicalIncidentStatus from, CyberPhysicalIncidentStatus to)
        : base($"Cannot transition CyberPhysicalIncident from '{from}' to '{to}'.") { }
}
