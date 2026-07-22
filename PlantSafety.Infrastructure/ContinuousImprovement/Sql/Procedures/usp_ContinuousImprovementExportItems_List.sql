-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.Export
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_ContinuousImprovementExportItems_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IExportContinuousImprovementRepository.GenerateExportAsync.
--                  Unions the CAPA and RCA source tables into a single flat
--                  export row set. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_ContinuousImprovementExportItems_List]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @FromUtc     DATETIME2(3) = NULL,
    @ToUtc       DATETIME2(3),
    @Status      NVARCHAR(50) = NULL,
    @MaxRecords  INT
AS
SET NOCOUNT ON;
BEGIN
    SELECT TOP (@MaxRecords) *
    FROM (
        SELECT
            CapaId       AS Id,
            'CAPA'       AS ResourceType,
            CapaId       AS ResourceId,
            Title        AS Title,
            Status       AS Status,
            DueDateUtc   AS EffectiveAtUtc,
            AssignedTo   AS OwnerId,
            SiteId,
            TenantId
        FROM [ContinuousImprovement].[CAPA]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR DueDateUtc >= @FromUtc)
          AND DueDateUtc <= @ToUtc
          AND (@Status IS NULL OR Status = @Status)

        UNION ALL

        SELECT
            RcaId                                   AS Id,
            'RCA'                                    AS ResourceType,
            RcaId                                    AS ResourceId,
            LEFT(ProblemStatement, 200)               AS Title,
            Status                                    AS Status,
            InitiatedAtUtc                            AS EffectiveAtUtc,
            OwnerUserId                               AS OwnerId,
            SiteId,
            TenantId
        FROM [ContinuousImprovement].[RCA]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR InitiatedAtUtc >= @FromUtc)
          AND InitiatedAtUtc <= @ToUtc
          AND (@Status IS NULL OR Status = @Status)
    ) AS Combined
    ORDER BY EffectiveAtUtc DESC;
END
GO
