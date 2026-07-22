-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SiteOrientation_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single SiteOrientation row by identifier, backing
--                  ISiteOrientationRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_SiteOrientation_GetById]
(
    @SiteOrientationId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SiteOrientationId], [TenantId], [SiteId], [Status], [OrientationTitle], [ContractorName], [ConductedByUserId], [DeliveredAtUtc], [AttendeeCount], [ComprehensionScore], [PassingScoreThreshold], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[SiteOrientation]
    WHERE [SiteOrientationId] = @SiteOrientationId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
