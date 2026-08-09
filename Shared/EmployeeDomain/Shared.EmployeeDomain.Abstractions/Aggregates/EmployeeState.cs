// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Aggregates
//  File:           EmployeeState.cs
//  Created:        2026-08-08
//  Description:    The immutable data snapshot of an EmployeeAggregate at a
//                  point in time. EmployeeAggregate is the only thing
//                  allowed to construct/replace an EmployeeState -- callers
//                  read it, they don't build it directly.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Injury;
using Shared.EmployeeDomain.Abstractions.Scoring;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Aggregates;

/// <summary>The immutable data snapshot of an <see cref="EmployeeAggregate"/>.</summary>
public sealed record EmployeeState(
    EmployeeId EmployeeId,
    string Name,
    EmployeeRole Role,
    EmployeeTrainingProfile TrainingProfile,
    IReadOnlyList<EmployeeCertification> Certifications,
    IReadOnlyList<EmployeeExposureRecord> ExposureRecords,
    EmployeeInjuryProfile InjuryProfile,
    EmployeeSafetyScore SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
