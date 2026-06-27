-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Template_Insert.sql
--  Created:        2026-06-17
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Canonical stored-procedure template for Insert operations.
--                  Copy and replace [Schema] and [Table] placeholders.
--                  All procs MUST accept @TenantId, @CorrelationId, @ActorId, @NowUtc.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Schema].[usp_Template_Insert]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    -- TODO: add module-specific parameters here
    @Id             UNIQUEIDENTIFIER OUTPUT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        SET @Id = NEWID();

        INSERT INTO [Schema].[Table]
            (Id, TenantId, CreatedBy, CreatedAt)
        VALUES
            (@Id, @TenantId, @ActorId, @NowUtc);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
