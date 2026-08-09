// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinEmployee.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: a read-only virtual snapshot of a real
//                  Shared.EmployeeDomain employee, for simulation purposes
//                  only.
// ============================================================================
namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a real Shared.EmployeeDomain employee.</summary>
public sealed record DigitalTwinEmployee(
    Guid EmployeeId,
    string Name,
    int InjuryCount);
