// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety
//  Layer:          Infrastructure
//  File:           EmployeeSafetyRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  IEmployeeSafetyRepository (declared with zero members in the Domain layer;
//                  the real persistence surface for this module lives entirely in the 7
//                  sub-component repositories under EmployeeSafety/*/Persistence). Provided
//                  purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.Repositories;

namespace PlantSafety.Infrastructure.EmployeeSafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="IEmployeeSafetyRepository"/> marker.</summary>
public sealed class EmployeeSafetyRepository : IEmployeeSafetyRepository;
