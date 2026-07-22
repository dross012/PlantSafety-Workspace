// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PermitTemplate aggregate root. Templates are the
//                  reusable definitions ("Permit templates for each category") behind the five
//                  core permit categories (Hot Work, Confined Space Entry, Electrical/Isolation,
//                  Working at Height, Excavation & Ground Disturbance) referenced by "System
//                  Features for PTW Module" -> "Permit Management".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Enums;

/// <summary>Lifecycle states for the <see cref="PermitTemplate"/> aggregate root.</summary>
public enum PermitTemplateStatus
{
    /// <summary>Template has been drafted and is not yet available for issuing new permits.</summary>
    Draft = 0,

    /// <summary>Template is published and may be used to request/issue permits of this category.</summary>
    Active = 1,

    /// <summary>Template is deprecated: existing permits may complete, but no new permits may be requested against it.</summary>
    Deprecated = 2,

    /// <summary>Template has been retired and is no longer usable for any purpose.</summary>
    Retired = 3,

    /// <summary>Template has been archived for long-term compliance retention.</summary>
    Archived = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Draft"/>.</summary>
    [Obsolete("Use Draft. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Draft,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Retired,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Retired,
}
