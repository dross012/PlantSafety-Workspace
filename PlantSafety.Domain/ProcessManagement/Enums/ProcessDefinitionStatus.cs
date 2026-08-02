// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status vocabulary for the ProcessDefinition aggregate root. Phase 4
//                  full build-out: replaces the previously-unstructured "string Status" field
//                  (Draft/Active/UnderReview/Retired, doc-comment-only vocabulary) with this
//                  documented enum. Archived is added as the platform-standard terminal
//                  retention state, consistent with every other module in this build-out.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Enums;

/// <summary>Lifecycle states for the <see cref="ProcessDefinition"/> aggregate root.</summary>
public enum ProcessDefinitionStatus
{
    /// <summary>Process definition is being authored and has not yet entered review.</summary>
    Draft = 0,

    /// <summary>Process definition has been submitted and is awaiting approval.</summary>
    UnderReview = 1,

    /// <summary>Process definition has been approved and is in effect.</summary>
    Active = 2,

    /// <summary>Process definition has been withdrawn from active use.</summary>
    Retired = 3,

    /// <summary>Process definition has been archived for long-term retention.</summary>
    Archived = 4,
}
