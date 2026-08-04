// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the CommunityContact module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the CommunityContact module.</summary>
public abstract class CommunityContactDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CommunityContactDomainException(
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
