// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Enums
//  File:           EmployeeRole.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: the operational role vocabulary for an
//                  employee within a facility. Additive-only: new members
//                  may be appended, existing members must never be
//                  renumbered or removed once published.
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.Enums;

/// <summary>An employee's operational role within a facility.</summary>
public enum EmployeeRole
{
    /// <summary>Operates production/process equipment.</summary>
    Operator,

    /// <summary>Supervises a crew or shift.</summary>
    Supervisor,

    /// <summary>Performs skilled technical work (e.g. instrumentation, electrical).</summary>
    Technician,

    /// <summary>Performs maintenance/repair work on machines or facilities.</summary>
    Maintenance,

    /// <summary>Dedicated safety/EHS personnel.</summary>
    Safety,

    /// <summary>A non-employee contractor working on-site.</summary>
    Contractor,

    /// <summary>Administrative/office personnel.</summary>
    Administrator,

    /// <summary>A role that does not match any of the above -- the classification default, never a data-entry error.</summary>
    Other,
}
