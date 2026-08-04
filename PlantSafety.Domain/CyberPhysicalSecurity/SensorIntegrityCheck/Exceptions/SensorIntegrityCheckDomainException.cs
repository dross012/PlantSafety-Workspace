// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for all SensorIntegrityCheck domain rule violations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Exceptions;

/// <summary>Base exception for SensorIntegrityCheck domain rule violations.</summary>
public class SensorIntegrityCheckDomainException : Exception
{
    /// <summary>Creates a new <see cref="SensorIntegrityCheckDomainException"/>.</summary>
    public SensorIntegrityCheckDomainException(string message) : base(message) { }
}

/// <summary>Raised when a SensorIntegrityCheck cannot be located by identity.</summary>
public sealed class SensorIntegrityCheckNotFoundException : SensorIntegrityCheckDomainException
{
    /// <summary>Creates a new <see cref="SensorIntegrityCheckNotFoundException"/>.</summary>
    public SensorIntegrityCheckNotFoundException(Guid sensorIntegrityCheckId)
        : base($"SensorIntegrityCheck '{sensorIntegrityCheckId}' was not found.") { }
}
