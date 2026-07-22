// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing
//  Layer:          Domain
//  File:           AuditDisciplineType.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum enumerating the 8 canonical feature-level audit
//                  disciplines defined by the PlantSafety Program Structure
//                  "Auditing Module" (Operations, Systems, Procedures, Quality
//                  Control, Regulatory Compliance, Efficiency, Inspecting, and
//                  Facility Assessment auditing). This module evaluates
//                  manufacturing operations, safety systems, environmental
//                  controls, and regulatory adherence -- it is explicitly NOT
//                  an application/software/cybersecurity audit.
//                  Additive-only evolution: new disciplines may be appended;
//                  existing codes/ids/display names must never change meaning.
// ============================================================================
namespace PlantSafety.Domain.Auditing.SmartEnums;

/// <summary>
/// Canonical feature-level audit discipline. Strong-typed Smart Enum carrying stable
/// <see cref="Id"/>/<see cref="Code"/> pairs plus the discipline's typical output
/// artifacts, used to classify <c>AuditEngagement</c> aggregates.
/// </summary>
public sealed class AuditDisciplineType
{
    /// <summary>Stable numeric identifier (1-based, matches the source document ordering).</summary>
    public int Id { get; }

    /// <summary>Stable machine-readable code (PascalCase, no abbreviations).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Narrative summary of the discipline's typical outputs (compliance-safe, non-PII).</summary>
    public string TypicalOutputs { get; }

    private AuditDisciplineType(int id, string code, string displayName, string typicalOutputs)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
        TypicalOutputs = typicalOutputs;
    }

    /// <summary>Day-to-day production activity and operator behavior audits.</summary>
    public static readonly AuditDisciplineType Operations = new(1, "Operations", "Operations Auditing",
        "Operational compliance score, SOP deviation reports, corrective action assignments.");

    /// <summary>Evaluates the effectiveness and consistency of overarching management systems (e.g. QMS).</summary>
    public static readonly AuditDisciplineType Systems = new(2, "Systems", "Systems Auditing",
        "QMS health score, system gap analysis, improvement roadmap.");

    /// <summary>Ensures documented procedures are accurate, up to date, and followed correctly.</summary>
    public static readonly AuditDisciplineType Procedures = new(3, "Procedures", "Procedures Auditing",
        "Procedure compliance score, SOP revision recommendations, training updates.");

    /// <summary>Focuses on product quality, defect prevention, and process consistency.</summary>
    public static readonly AuditDisciplineType QualityControl = new(4, "QualityControl", "Quality Control Auditing",
        "Quality performance score, defect reduction plan, QC process improvements.");

    /// <summary>Ensures adherence to all applicable laws, regulations, and industry standards.</summary>
    public static readonly AuditDisciplineType RegulatoryCompliance = new(5, "RegulatoryCompliance", "Regulatory Compliance Auditing",
        "Compliance status report, regulatory gap analysis, required corrective actions, audit-ready documentation.");

    /// <summary>Evaluates how effectively resources, time, and equipment are used.</summary>
    public static readonly AuditDisciplineType Efficiency = new(6, "Efficiency", "Efficiency Auditing",
        "Efficiency score, lean improvement recommendations, waste reduction plan.");

    /// <summary>Focused inspections of critical safety and operational areas.</summary>
    public static readonly AuditDisciplineType Inspecting = new(7, "Inspecting", "Inspecting Audits",
        "Inspection findings, hazard identification logs, corrective action assignments.");

    /// <summary>Evaluates the physical condition and safety of the entire facility.</summary>
    public static readonly AuditDisciplineType FacilityAssessment = new(8, "FacilityAssessment", "Facility Assessment Auditing",
        "Facility condition score, maintenance priority list, environmental compliance report.");

    /// <summary>Default discipline used when no explicit discipline has been assigned.</summary>
    public static readonly AuditDisciplineType Default = Operations;

    /// <summary>All defined audit disciplines, in canonical (source-document) order.</summary>
    public static readonly IReadOnlyList<AuditDisciplineType> All =
    [
        Operations,
        Systems,
        Procedures,
        QualityControl,
        RegulatoryCompliance,
        Efficiency,
        Inspecting,
        FacilityAssessment,
    ];

    /// <summary>Resolves an <see cref="AuditDisciplineType"/> from its <see cref="Code"/>. Throws if not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known discipline.</exception>
    public static AuditDisciplineType From(string code)
        => TryParse(code, out var value)
            ? value!
            : throw new ArgumentException($"'{code}' is not a valid AuditDisciplineType code.", nameof(code));

    /// <summary>Attempts to resolve an <see cref="AuditDisciplineType"/> from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AuditDisciplineType? value)
    {
        value = All.FirstOrDefault(d => string.Equals(d.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised discipline code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
