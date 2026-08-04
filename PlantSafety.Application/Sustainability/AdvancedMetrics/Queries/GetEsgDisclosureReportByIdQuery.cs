// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetEsgDisclosureReportByIdQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving a single ESG disclosure report by identity.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

/// <summary>Retrieves a single ESG disclosure report read-model record by identity.</summary>
public sealed record GetEsgDisclosureReportByIdQuery(
    Guid TenantId,
    Guid EsgDisclosureReportId) : IRequest<Result<EsgDisclosureReportDto>>;
