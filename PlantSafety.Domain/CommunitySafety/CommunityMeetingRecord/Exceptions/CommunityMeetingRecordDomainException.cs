// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the CommunityMeetingRecord
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the CommunityMeetingRecord module.</summary>
public abstract class CommunityMeetingRecordDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CommunityMeetingRecordDomainException(
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
