// ============================================================================
//  Project:        Enterprise.DataLake
//  Module:         Abstractions.Enums
//  File:           DataLakeRecordType.cs
//  Created:        2026-08-08
//  Description:    Phase D.2 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): the vocabulary of record kinds the
//                  enterprise data lake stores -- one per real bounded
//                  context from Paths B/C, plus Prediction for Path C.11's
//                  predictive model outputs. Additive-only: new members
//                  may be appended, existing members must never be
//                  renumbered or removed once published.
// ============================================================================
namespace Enterprise.DataLake.Abstractions.Enums;

/// <summary>The kind of record stored in the enterprise data lake.</summary>
public enum DataLakeRecordType
{
    /// <summary>A Shared.FacilityDomain facility event (Path B).</summary>
    Facility,

    /// <summary>A Shared.EmployeeDomain employee event (Path C).</summary>
    Employee,

    /// <summary>A Shared.MachineDomain machine event (Path C).</summary>
    Machine,

    /// <summary>A Shared.FacilityDomain hazard/hazard-propagation event (Path B/C).</summary>
    Hazard,

    /// <summary>A correlated real domain event (Path B/C).</summary>
    Event,

    /// <summary>A Shared.WorkflowEngine workflow decision (Path C).</summary>
    Workflow,

    /// <summary>A Shared.PredictiveModels predictive model output (Path C).</summary>
    Prediction,
}
