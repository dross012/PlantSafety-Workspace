-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_RCA_GetRcaRecords.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRCARepository.GetRcaRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_RCA_GetRcaRecords]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RcaId,
        TenantId,
        SiteId,
        ProblemStatement,
        Methodology,
        Status,
        InitiatedAtUtc,
        OwnerUserId
    FROM [ContinuousImprovement].[RCA]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND InitiatedAtUtc >= @FromUtc
      AND InitiatedAtUtc <= @ToUtc
    ORDER BY InitiatedAtUtc DESC;
END
GO
