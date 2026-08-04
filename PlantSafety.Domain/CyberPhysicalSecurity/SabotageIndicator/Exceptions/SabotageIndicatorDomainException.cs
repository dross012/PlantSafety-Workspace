// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for all SabotageIndicator domain rule violations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Exceptions;

/// <summary>Base exception for SabotageIndicator domain rule violations.</summary>
public class SabotageIndicatorDomainException : Exception
{
    /// <summary>Creates a new <see cref="SabotageIndicatorDomainException"/>.</summary>
    public SabotageIndicatorDomainException(string message) : base(message) { }
}

/// <summary>Raised when a SabotageIndicator cannot be located by identity.</summary>
public sealed class SabotageIndicatorNotFoundException : SabotageIndicatorDomainException
{
    /// <summary>Creates a new <see cref="SabotageIndicatorNotFoundException"/>.</summary>
    public SabotageIndicatorNotFoundException(Guid sabotageIndicatorId)
        : base($"SabotageIndicator '{sabotageIndicatorId}' was not found.") { }
}
