// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the HazardousWasteManagement V1 controller. Each record carries
//   only the domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId,
//   TenantId, ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate
//   identifiers are bound from the route, not the body.

namespace PlantSafety.Api.HazardousWasteManagement.Contracts.Requests;

// ── WasteStreams ──────────────────────────────────────────────────────────────
public sealed record CharacterizeWasteStreamRequest(Guid? SiteId, string StreamName, string EpaWasteCode, string HazardClass, string GeneratorEpaId);

// ── AccumulationAreas / WasteContainers ──────────────────────────────────────
public sealed record OpenAccumulationContainerRequest(
    Guid? SiteId, Guid AccumulationAreaId, Guid WasteStreamId, string ContainerLabel, decimal CapacityGallons, int RegulatoryTimeLimitDays);

// ── Manifests ─────────────────────────────────────────────────────────────────
public sealed record GenerateManifestRequest(Guid? SiteId, string ManifestNumber, string GeneratorEpaId, Guid WasteStreamId);
public sealed record RecordTransporterPickupRequest(Guid TransporterId);
public sealed record RecordTsdfReceiptRequest(Guid TsdfFacilityId);
public sealed record FlagManifestDiscrepancyRequest(string DiscrepancyReason);

// ── DisposalCertificates ─────────────────────────────────────────────────────
public sealed record RecordDisposalCertificateRequest(
    Guid? SiteId, Guid ManifestId, Guid TsdfFacilityId, string DisposalMethod, DateTime DisposalDateUtc);

// ── Shared query filters ─────────────────────────────────────────────────────
public sealed record HazardousWasteManagementSiteQueryRequest(Guid? SiteId);
