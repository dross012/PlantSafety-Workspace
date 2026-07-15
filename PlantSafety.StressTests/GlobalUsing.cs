// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         StressTests
//  Layer:          Tests (Stress)
//  Description:    Global usings for the Stress test suite (PLNT-STRS-*).
//                  Exercises performance_stress_manual.md SR-series rules:
//                  SR10 (max queue depth 10,000), SR11 (drop low-priority at
//                  >=90% depth), SR30/SR31 (circuit breaker trips after 5
//                  consecutive failures, half-open probe), PR10/PR11 (2x
//                  sustained throughput, 5x burst for >=30s).
// ============================================================================
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Threading.Channels;
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Common.Types.StrongIds;
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
