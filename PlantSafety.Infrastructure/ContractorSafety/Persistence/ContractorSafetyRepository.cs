// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety
//  Layer:          Infrastructure
//  File:           ContractorSafetyRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  IContractorSafetyRepository (declared with zero members in the Domain
//                  layer; the real persistence surface for this module lives entirely in the
//                  5 sub-component repositories under ContractorSafety/*/Persistence).
//                  Provided purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.Repositories;

namespace PlantSafety.Infrastructure.ContractorSafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="IContractorSafetyRepository"/> marker.</summary>
public sealed class ContractorSafetyRepository : IContractorSafetyRepository;
