// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CompatibilityTests
//  Layer:          Tests (Compatibility / Interoperability / Backward Compat)
//  Description:    Global usings for the Compatibility test suite covering
//                  the three formally distinct 17-type-manual test types
//                  COMPAT, INTEROP and BKWD (tests_rules_design_manual.md),
//                  consolidated into one project per the task's stated
//                  granularity guidance ("Interoperability/Backward
//                  Compatibility could share a project"). Implemented as
//                  reflection-based API-CONTRACT-SHAPE tests over the
//                  compiled PlantSafety.Api assembly (no live host/DB
//                  required), per the task's lighter-weight guidance for
//                  this category.
// ============================================================================
global using System.Reflection;
global using Asp.Versioning;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Routing;
global using Xunit;
