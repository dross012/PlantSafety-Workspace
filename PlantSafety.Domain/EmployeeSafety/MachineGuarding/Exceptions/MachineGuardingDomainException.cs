// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the MachineGuarding module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the MachineGuarding module.</summary>
public abstract class MachineGuardingDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected MachineGuardingDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}