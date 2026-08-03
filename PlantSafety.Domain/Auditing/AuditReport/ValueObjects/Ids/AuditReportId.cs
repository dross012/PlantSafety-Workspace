// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AuditReport aggregate root
//                  (Handbook Phase 5 §6.2 printable audit reports). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AuditReport aggregate root.</summary>
public readonly record struct AuditReportId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AuditReportId wrapping a non-empty GUID.</summary>
    public AuditReportId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditReportId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditReportId using Uuid7.</summary>
    public static AuditReportId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AuditReportId.</summary>
    public static AuditReportId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
