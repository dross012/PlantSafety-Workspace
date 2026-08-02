// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           GetCertificationByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Certification read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.Models;

namespace PlantSafety.Application.Compliance.Certification.Queries;

/// <summary>Retrieves a single Certification read-model record by identity.</summary>
public sealed record GetCertificationByIdQuery(
    Guid TenantId,
    Guid CertificationId) : IRequest<Result<CertificationItemDto>>;
