// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           GetContractorVisitorByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single ContractorVisitor read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorVisitor.Models;

namespace PlantSafety.Application.ContractorVisitor.Queries;

/// <summary>Retrieves a single ContractorVisitor read-model record by identity.</summary>
public sealed record GetContractorVisitorByIdQuery(
    Guid TenantId,
    Guid ContractorVisitorId) : IRequest<Result<ContractorVisitorItemDto>>;
