-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.IndustrialHygiene
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IndustrialHygiene_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[IndustrialHygieneReadModel],
--                  backing IIndustrialHygieneRepository.GetIndustrialHygieneAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_IndustrialHygiene_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [IndustrialHygieneSampleId],
        [TenantId],
        [SiteId],
        [SampleType],
        [ExposureLevel],
        [Status],
        [SampledAtUtc],
        [SampledBy]
FROM [Operational].[IndustrialHygieneReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SampledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SampledAtUtc] DESC;
GO
