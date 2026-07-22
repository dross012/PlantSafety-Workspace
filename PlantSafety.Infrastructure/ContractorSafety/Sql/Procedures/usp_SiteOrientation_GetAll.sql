-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SiteOrientation_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted SiteOrientation rows, backing
--                  ISiteOrientationRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_SiteOrientation_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SiteOrientationId], [TenantId], [SiteId], [Status], [OrientationTitle], [ContractorName], [ConductedByUserId], [DeliveredAtUtc], [AttendeeCount], [ComprehensionScore], [PassingScoreThreshold], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[SiteOrientation]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
