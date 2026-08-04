// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeEventDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for all ControlChangeEvent domain rule violations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Exceptions;

/// <summary>Base exception for ControlChangeEvent domain rule violations.</summary>
public class ControlChangeEventDomainException : Exception
{
    /// <summary>Creates a new <see cref="ControlChangeEventDomainException"/>.</summary>
    public ControlChangeEventDomainException(string message) : base(message) { }
}

/// <summary>Raised when a ControlChangeEvent cannot be located by identity.</summary>
public sealed class ControlChangeEventNotFoundException : ControlChangeEventDomainException
{
    /// <summary>Creates a new <see cref="ControlChangeEventNotFoundException"/>.</summary>
    public ControlChangeEventNotFoundException(Guid controlChangeEventId)
        : base($"ControlChangeEvent '{controlChangeEventId}' was not found.") { }
}
