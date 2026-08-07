-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Placarding
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatContainer_GetAllActive.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_HazmatContainer rows, ordered by CreatedAtUtc.
--                  Backs IHazmatContainerRepository.ListAsync, mirroring usp_Permits_GetAllActive.sql
--                  (the Domain interface carries no tenant/site scoping parameter to push down to
--                  SQL, so filtering is applied in-memory over the candidate set returned here, same
--                  as the Permits precedent).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatContainer_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        HazmatContainerId, TenantId, SiteId, Status, ContainerType, MaterialId, PackagingSpecId,
        Markings, Labels, Placards, OrientationArrowsRequired, CompatibilityGroup,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[PHMSA_HazmatContainer]
    WHERE IsDeleted = 0
    ORDER BY CreatedAtUtc;
END
GO
