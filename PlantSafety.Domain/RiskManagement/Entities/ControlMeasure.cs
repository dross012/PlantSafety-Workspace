// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           ControlMeasure.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the Risk aggregate representing a single applied control
//                  measure from the Hierarchy of Controls. Grounded in Full Risk Management
//                  Process PHASE 2, Steps 5-6: controls "must be applied in order of
//                  effectiveness", with the system "requiring justification when lower-level
//                  controls are used", and "Each corrective action must include: Responsible
//                  person, Deadline, Verification steps, Evidence of completion".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Entities;

/// <summary>A single control measure applied against a Risk, drawn from the Hierarchy of Controls.</summary>
public sealed class ControlMeasure
{
    /// <summary>Unique identity of this control measure.</summary>
    public ControlMeasureId ControlMeasureId { get; private set; }

    /// <summary>Identity of the parent Risk aggregate.</summary>
    public RiskId RiskId { get; private set; }

    /// <summary>Hierarchy-of-Controls rung this measure represents.</summary>
    public ControlHierarchyLevel HierarchyLevel { get; private set; }

    /// <summary>Description of the control measure applied.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Justification for using this control level instead of a higher-effectiveness one.
    /// Required (enforced by the aggregate) whenever <see cref="ControlHierarchyLevel.RequiresJustification"/> is true.
    /// </summary>
    public string? Justification { get; private set; }

    /// <summary>Identity of the person responsible for implementing this control.</summary>
    public UserId ResponsiblePersonId { get; private set; }

    /// <summary>UTC instant this control measure was implemented.</summary>
    public UtcTimestamp ImplementedAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private ControlMeasure() { }

    /// <summary>Creates a new control measure record.</summary>
    public ControlMeasure(
        RiskId riskId,
        ControlHierarchyLevel hierarchyLevel,
        string description,
        string? justification,
        UserId responsiblePersonId,
        UtcTimestamp implementedAtUtc)
    {
        ControlMeasureId = ControlMeasureId.New();
        RiskId = riskId;
        HierarchyLevel = hierarchyLevel;
        Description = (description ?? string.Empty).Trim();
        Justification = string.IsNullOrWhiteSpace(justification) ? null : justification.Trim();
        ResponsiblePersonId = responsiblePersonId;
        ImplementedAtUtc = implementedAtUtc;
    }
}
