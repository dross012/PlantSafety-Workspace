// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           GetHazmatShipmentQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single HazmatShipment read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Queries;

/// <summary>Retrieves a single HazmatShipment read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazmatShipmentId">Identity of the HazmatShipment record to retrieve.</param>
public sealed record GetHazmatShipmentQuery(
    Guid TenantId,
    Guid HazmatShipmentId) : IRequest<Result<HazmatShipmentDto>>;
