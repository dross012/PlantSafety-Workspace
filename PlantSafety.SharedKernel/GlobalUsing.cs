// ============================================================================
//  Project:        PlantSafety.Platform
//  Layer:          SharedKernel
//  File:           GlobalUsing.cs
//  Created:        2026-05-26
//  Updated:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Project-wide global using directives for PlantSafety.SharedKernel.
//                  Covers System, Microsoft, and all SharedKernel namespaces.
//                  Additive-only evolution.
// ============================================================================

// ---- System ----------------------------------------------------------------
global using System.Collections.Concurrent;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Security.Cryptography;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;

// ---- Microsoft -------------------------------------------------------------
global using Microsoft.Extensions.DependencyInjection;

// ---- SharedKernel: Identifiers ---------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;

// ---- SharedKernel: Primitives ----------------------------------------------
global using PlantSafety.SharedKernel.Primitives.Errors;
global using PlantSafety.SharedKernel.Primitives.Exceptions;
global using PlantSafety.SharedKernel.Primitives.StrongIds;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;

// ---- SharedKernel: Events --------------------------------------------------
global using PlantSafety.SharedKernel.Events.Base;
global using PlantSafety.SharedKernel.Events.Metadata;

// ---- SharedKernel: Serialization -------------------------------------------
global using PlantSafety.SharedKernel.Serialization.Converters;

// ---- SharedKernel: Time ----------------------------------------------------
global using PlantSafety.SharedKernel.Time;
