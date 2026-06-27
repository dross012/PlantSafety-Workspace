// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests
//  Layer:          Tests (Unit)
//  File:           GlobalUsing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Project-wide global usings for all unit test files.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
// ---- System ----------------------------------------------------------------
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;

// ---- xunit -----------------------------------------------------------------
global using Xunit;

// ---- FluentAssertions -------------------------------------------------------
global using FluentAssertions;

// ---- FluentValidation -------------------------------------------------------
global using FluentValidation;
global using FluentValidation.TestHelper;

// ---- NSubstitute -----------------------------------------------------------
global using NSubstitute;

// ---- Microsoft -------------------------------------------------------------
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;

// ---- PlantSafety: SharedKernel ---------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;

// ---- PlantSafety: Domain — Common ------------------------------------------
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Common.Base;
global using PlantSafety.Domain.Common.Exceptions;
global using PlantSafety.Domain.Common.Rules;
global using PlantSafety.Domain.Common.Types.StrongIds;

// ---- PlantSafety: Domain — Operational.Alerts (canonical reference module) -
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Events;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

// ---- Tests: Common ---------------------------------------------------------
global using PlantSafety.Tests.Common.TestBase;
global using PlantSafety.Tests.Common.Builders;
global using PlantSafety.Tests.Common.Fakes;
