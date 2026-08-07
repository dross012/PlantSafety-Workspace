-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Packaging
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAPackagingSpecification_GetAllActive.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_PackagingSpecification rows, ordered by
--                  CreatedAtUtc. Backs IPackagingSpecificationRepository.ListAsync, mirroring
--                  usp_Permits_GetAllActive.sql (the Domain interface carries no tenant/site scoping
--                  parameter to push down to SQL, so filtering is applied in-memory over the
--                  candidate set returned here, same as the Permits precedent). Optional @DOTSpec
--                  filter lets callers narrow to a specific DOT specification code without an extra
--                  round trip.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAPackagingSpecification_GetAllActive]
    @DOTSpec NVARCHAR(20) = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PackagingSpecificationId, TenantId, SiteId, Status, DOTSpec, PerformanceTests, MaxCapacity, CapacityUnit,
        ClosureInstructions, ReuseRestrictions, AssociatedMaterialId,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[PHMSA_PackagingSpecification]
    WHERE IsDeleted = 0
      AND (@DOTSpec IS NULL OR DOTSpec = @DOTSpec)
    ORDER BY CreatedAtUtc;
END
GO
