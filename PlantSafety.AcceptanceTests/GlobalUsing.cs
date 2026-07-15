// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AcceptanceTests
//  Layer:          Tests (Acceptance / UAT)
//  Description:    Global usings for the UAT/Acceptance suite (PLNT-UAT-*).
//                  Written business-readable, Given/When/Then style, over the
//                  safety-critical Alert feature -- the acceptance criteria a
//                  plant Safety Officer would sign off against per
//                  tests_rules_design_manual.md's UAT test type and Appendix
//                  A Test Case template (mandatory fields: preconditions,
//                  steps, expected result).
// ============================================================================
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects;
global using PlantSafety.Domain.Common.Types.StrongIds;
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
