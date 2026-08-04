-- ============================================================================
--  Module: HazardousWasteManagement.WasteStreams | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteStreams_GetById.sql | Created: 2026-08-04
--  Description: Returns a single WasteStreams row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_GetById]
(
    @WasteStreamId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[WasteStreams]
    WHERE [WasteStreamId] = @WasteStreamId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
