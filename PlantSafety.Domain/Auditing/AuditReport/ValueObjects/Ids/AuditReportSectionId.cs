// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportSectionId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for an AuditReportSection child entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for an AuditReportSection entity.</summary>
public readonly record struct AuditReportSectionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AuditReportSectionId wrapping a non-empty GUID.</summary>
    public AuditReportSectionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditReportSectionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditReportSectionId using Uuid7.</summary>
    public static AuditReportSectionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AuditReportSectionId.</summary>
    public static AuditReportSectionId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
