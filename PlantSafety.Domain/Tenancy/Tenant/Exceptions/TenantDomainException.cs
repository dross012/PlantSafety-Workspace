// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantDomainException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised by the Tenant submodule.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.Tenancy.Tenant.Exceptions;

/// <summary>Abstract base for all domain exceptions raised by the Tenant submodule.</summary>
public abstract class TenantDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected TenantDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}
