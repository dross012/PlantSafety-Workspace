// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single OtAsset read-model record by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Queries;

/// <summary>Retrieves a single OtAsset read-model record by identity.</summary>
public sealed record GetOtAssetByIdQuery(Guid TenantId, Guid OtAssetId) : IRequest<Result<OtAssetDto>>;
