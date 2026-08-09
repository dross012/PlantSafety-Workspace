// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Repositories
//  File:           IEmployeeService.cs
//  Created:        2026-08-08
//  Description:    The outward-facing application service over
//                  EmployeeAggregate/IEmployeeRepository -- callers outside
//                  Shared.EmployeeDomain depend on this, not on the
//                  aggregate or repository directly.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Repositories;

/// <summary>Application service orchestrating EmployeeAggregate commands against IEmployeeRepository.</summary>
public interface IEmployeeService
{
    /// <summary>Creates a new employee and returns its resulting state.</summary>
    Task<EmployeeState> CreateEmployeeAsync(string name, EmployeeRole role, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing employee's metadata. Returns null if no employee with that id exists.</summary>
    Task<EmployeeState?> UpdateEmployeeMetadataAsync(EmployeeId employeeId, string name, EmployeeRole role, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing employee's training profile. Returns null if no employee with that id exists.</summary>
    Task<EmployeeState?> UpdateTrainingProfileAsync(EmployeeId employeeId, IReadOnlyList<string> completedTrainingCodes, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing employee's certifications. Returns null if no employee with that id exists.</summary>
    Task<EmployeeState?> UpdateCertificationsAsync(EmployeeId employeeId, IReadOnlyList<EmployeeCertification> certifications, CancellationToken cancellationToken = default);

    /// <summary>Records a hazard exposure for an existing employee. Returns null if no employee with that id exists.</summary>
    Task<EmployeeState?> RecordExposureAsync(EmployeeId employeeId, EmployeeExposureRecord exposureRecord, CancellationToken cancellationToken = default);

    /// <summary>Records an injury for an existing employee. Returns null if no employee with that id exists.</summary>
    Task<EmployeeState?> RecordInjuryAsync(EmployeeId employeeId, string description, CancellationToken cancellationToken = default);

    /// <summary>Returns the employee with the given id, or null if none exists.</summary>
    Task<EmployeeState?> GetEmployeeAsync(EmployeeId employeeId, CancellationToken cancellationToken = default);

    /// <summary>Returns every employee, in no particular order.</summary>
    Task<IReadOnlyList<EmployeeState>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
}
