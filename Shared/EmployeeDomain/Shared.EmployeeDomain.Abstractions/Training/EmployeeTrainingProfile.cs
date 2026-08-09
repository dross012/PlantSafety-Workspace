// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Training
//  File:           EmployeeTrainingProfile.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: the set of training codes an employee has
//                  completed, as of a point in time. Replaced wholesale
//                  (not incrementally merged) by EmployeeAggregate.
//                  UpdateTrainingProfile, mirroring Shared.FacilityDomain's
//                  FacilityHazardProfile replace-not-merge convention.
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.Training;

/// <summary>The set of training codes an employee has completed, as of <see cref="UpdatedAtUtc"/>.</summary>
public sealed record EmployeeTrainingProfile(
    IReadOnlyList<string> CompletedTrainingCodes,
    DateTime UpdatedAtUtc);
