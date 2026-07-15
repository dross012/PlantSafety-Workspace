// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ScenarioTests
//  Layer:          Tests (Scenario / Workflow / End-to-End)
//  Description:    Global usings for the Scenario test suite covering the
//                  three formally distinct 17-type-manual test types SCEN,
//                  WKFL and E2E (tests_rules_design_manual.md), consolidated
//                  into one project per the task's stated granularity
//                  guidance ("Scenario/Workflow could share a project").
//
//                  NOTE ON SCOPE: this environment has no Docker daemon
//                  available (`docker version` fails), so the full HTTP+SQL
//                  Testcontainers-backed E2E path used by
//                  PlantSafety.IntegrationTests cannot execute here. These
//                  scenario/workflow/E2E tests instead drive the real
//                  production Domain aggregate state machines directly
//                  (no mocks of the aggregate itself) to exercise complete,
//                  multi-step, cross-status business workflows -- e.g. the
//                  full Alert lifecycle -- end-to-end at the domain layer.
//                  Full HTTP+DB E2E scenarios (Ch.14 of
//                  tests_rules_design_manual.md, e.g. E2E-SAFE-001) should be
//                  run via PlantSafety.IntegrationTests' Testcontainers host
//                  in a CI environment where Docker is present.
// ============================================================================
global using FluentAssertions;
global using Xunit;
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Common.Types.StrongIds;
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects;
global using PlantSafety.Domain.Operational.Alerts.Events;
