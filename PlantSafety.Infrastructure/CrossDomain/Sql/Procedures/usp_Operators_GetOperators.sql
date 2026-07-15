-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Operators
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Operators_GetOperators.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Operators],
--                  backing IOperatorsRepository.GetOperatorsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Operators_GetOperators]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [OperatorId],
        [TenantId],
        [SiteId],
        [FullName],
        [CertificationLevel],
        [AssignedMachineId],
        [Status],
        [CertifiedAtUtc],
        [CertifiedBy]
FROM [CrossDomain].[Operators]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CertifiedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CertifiedAtUtc] DESC;
GO
