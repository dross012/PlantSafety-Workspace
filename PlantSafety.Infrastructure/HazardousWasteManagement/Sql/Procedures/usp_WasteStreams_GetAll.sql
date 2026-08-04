-- ============================================================================
--  Module: HazardousWasteManagement.WasteStreams | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteStreams_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted WasteStreams rows, backing
--               IWasteStreamRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[WasteStreams]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
