-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Template_GetById.sql
--  Created:        2026-06-17
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Canonical read stored-procedure template (GetById).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Schema].[usp_Template_GetById]
    @TenantId   UNIQUEIDENTIFIER,
    @Id         UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY

    SELECT
        t.Id,
        t.TenantId,
        t.CreatedAt,
        t.CreatedBy
    FROM [Schema].[Table] AS t
    WHERE t.TenantId = @TenantId
      AND t.Id       = @Id;

END TRY
BEGIN CATCH
    THROW;
END CATCH;
GO
