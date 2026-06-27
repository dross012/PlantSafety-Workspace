// ============================================================================
//  Project:        PlantSafety.Platform
//  Layer:          Infrastructure
//  File:           GlobalUsing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Project-wide global using directives for PlantSafety.Infrastructure.
//      Covers System, Dapper, MassTransit, SharedKernel, Domain, and
//      Application namespaces so individual files need no explicit usings.
//      Additive-only evolution.
// ============================================================================

// ---- System ----------------------------------------------------------------
global using System.Data;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Collections.ObjectModel;

// ---- Microsoft -------------------------------------------------------------
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Data.SqlClient;

// ---- Dapper ----------------------------------------------------------------
global using Dapper;

// ---- MassTransit -----------------------------------------------------------
global using MassTransit;

// ---- SharedKernel ----------------------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;

// ---- Domain: Common --------------------------------------------------------
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Common.Types.StrongIds;

// ---- Domain: Operational.Alerts (canonical reference module) ---------------
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Events;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

// ---- Infrastructure: Common ------------------------------------------------
global using PlantSafety.Infrastructure.Common.Configuration;
global using PlantSafety.Infrastructure.Common.Constants;
global using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
global using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
global using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
global using PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;

// ---- Infrastructure: Messaging ---------------------------------------------
global using PlantSafety.Infrastructure.Messaging.Consumers;
