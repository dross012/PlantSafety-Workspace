-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AllInvestigations
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_AllInvestigations_GetInvestigations.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAllInvestigationsRepository.GetInvestigationsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Intelligence].[usp_AllInvestigations_GetInvestigations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        InvestigationId,
        TenantId,
        SiteId,
        IncidentReference,
        AiConfidenceScore,
        Summary,
        Status,
        OpenedAtUtc,
        AssignedTo
    FROM [Intelligence].[AllInvestigations]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND OpenedAtUtc >= @FromUtc
      AND OpenedAtUtc <= @ToUtc
    ORDER BY OpenedAtUtc DESC;
END
GO
