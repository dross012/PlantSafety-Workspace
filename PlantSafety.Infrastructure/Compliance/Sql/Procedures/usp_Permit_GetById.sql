-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Permit_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPermitRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @PermitId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
        ValidFromUtc, ValidToUtc, Status, ApprovedByActorId, ApprovedAtUtc, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Permit]
    WHERE PermitId = @PermitId AND TenantId = @TenantId AND IsDeleted = 0;
END
GO
