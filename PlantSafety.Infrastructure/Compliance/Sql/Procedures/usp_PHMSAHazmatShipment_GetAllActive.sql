-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.ShippingPapers
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatShipment_GetAllActive.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_HazmatShipment rows, ordered by CreatedAtUtc.
--                  Backs IHazmatShipmentRepository.ListAsync, mirroring usp_Permits_GetAllActive.sql
--                  (the Domain interface carries no tenant/site scoping parameter to push down to
--                  SQL, so filtering is applied in-memory over the candidate set returned here, same
--                  as the Permits precedent). Optional @Status and @Mode filters support a shipment
--                  dashboard that needs to narrow by lifecycle status and/or transport mode.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatShipment_GetAllActive]
    @Status NVARCHAR(20) = NULL,
    @Mode NVARCHAR(10) = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        HazmatShipmentId, TenantId, SiteId, Status, MaterialIds, PackagingIds,
        ShippingPaperBasicDescription, ShippingPaperEmergencyResponseRef, ShippingPaperCertification,
        ShippingPaperWasteManifestReference, ShippingPaperAdditionalModalRequirements, ShippingPaperGeneratedAtUtc,
        EmergencyResponseErgGuideNumber, EmergencyResponseContactName, EmergencyResponseContactPhone, EmergencyResponseGuidanceSummary,
        Mode, PlacardingRequirements, SegregationViolations,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[PHMSA_HazmatShipment]
    WHERE IsDeleted = 0
      AND (@Status IS NULL OR Status = @Status)
      AND (@Mode IS NULL OR Mode = @Mode)
    ORDER BY CreatedAtUtc;
END
GO
