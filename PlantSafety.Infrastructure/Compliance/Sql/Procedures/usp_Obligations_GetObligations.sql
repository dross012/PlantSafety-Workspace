-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Obligations
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Obligations_GetObligations.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IObligationsRepository.GetObligationsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Obligations_GetObligations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ObligationId,
        TenantId,
        SiteId,
        ObligationTitle,
        RegulatoryBody,
        Status,
        DueAtUtc,
        OwnerId
    FROM [Compliance].[Obligations]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DueAtUtc >= @FromUtc
      AND DueAtUtc <= @ToUtc
    ORDER BY DueAtUtc DESC;
END
GO
