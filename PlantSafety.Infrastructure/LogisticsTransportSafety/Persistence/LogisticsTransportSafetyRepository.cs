// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety
//  Layer:          Infrastructure
//  File:           LogisticsTransportSafetyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Trivial implementation of the empty top-level placeholder marker interface
//                  ILogisticsTransportSafetyRepository (declared with zero members in the Domain
//                  layer; the real persistence surface for this module lives entirely in the 6
//                  sub-component repositories under LogisticsTransportSafety/*/Persistence).
//                  Provided purely so the marker interface has a concrete, DI-registerable
//                  implementation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.Repositories;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.Persistence;

/// <summary>Trivial implementation of the empty top-level <see cref="ILogisticsTransportSafetyRepository"/> marker.</summary>
public sealed class LogisticsTransportSafetyRepository : ILogisticsTransportSafetyRepository;
