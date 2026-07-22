-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Contractor
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Contractor_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[ContractorReadModel],
--                  backing IContractorRepository.GetContractorAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Contractor_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ContractorRecordId],
        [TenantId],
        [SiteId],
        [ContractorName],
        [ServiceType],
        [Status],
        [OnboardedAtUtc],
        [ApprovedBy]
FROM [Operational].[ContractorReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [OnboardedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [OnboardedAtUtc] DESC;
GO
