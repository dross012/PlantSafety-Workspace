-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Classification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatMaterial_GetAllActive.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_HazmatMaterial rows, ordered by CreatedAtUtc.
--                  Backs IHazmatMaterialRepository.ListAsync, mirroring usp_Permits_GetAllActive.sql
--                  (the Domain interface carries no tenant/site scoping parameter to push down to
--                  SQL, so filtering is applied in-memory over the candidate set returned here, same
--                  as the Permits precedent). Optional @UNNumber filter lets callers narrow to a
--                  specific UN number's classification history without an extra round trip.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatMaterial_GetAllActive]
    @UNNumber NVARCHAR(20) = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        HazmatMaterialId, TenantId, SiteId, Status, UNNumber, ProperShippingName, HazardClass, PackingGroup,
        SpecialProvisions, ERGGuideNumber, IsMarinePollutant, IsTIH, TIHZone,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[PHMSA_HazmatMaterial]
    WHERE IsDeleted = 0
      AND (@UNNumber IS NULL OR UNNumber = @UNNumber)
    ORDER BY CreatedAtUtc;
END
GO
