-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Permits_GetRange.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPermitsRepository.GetPermitsAsync
--                  (Application-layer read model / GetPermitsQuery). Projects RowVersion as a
--                  string ETag so PermitsItemDto.ETag can round-trip back into a subsequent
--                  mutating command's IConcurrencyAwareCommand.ETag for read-before-write clients.
--                  Mirrors usp_PermitWorkflow_GetRange.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permits_GetRange]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitsId, TenantId, SiteId, Status, PermitType, PermitNumber, WorkDescription, Location, HazardsIdentified,
        RequestedByUserId, ApprovedByUserId, IssuedAtUtc, ExpiresAtUtc, RejectedReason, RejectedAtUtc, CompletedAtUtc,
        ExpiredAtUtc, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Permits]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO
