-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RenewalsDOT
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RenewalsDOT_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[RenewalsDOT],
--                  backing IDOTRepository.GetRenewalsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_RenewalsDOT_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RenewalId],
        [TenantId],
        [SiteId],
        [CredentialType],
        [ExpiresAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[RenewalsDOT]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ExpiresAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ExpiresAtUtc] DESC;
GO