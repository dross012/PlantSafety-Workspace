-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Process_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ProcessManagement].[Processes], backing
--                  IProcessRepository.CreateAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_Process_Create]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @ProcessId      UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @ProcessName    NVARCHAR(200),
    @Category       NVARCHAR(50),
    @OwnerId        UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [ProcessManagement].[Processes]
            ([ProcessId], [TenantId], [SiteId], [ProcessName], [Category], [Status], [Version],
             [OwnerId], [CreatedBy], [CreatedAt], [LastModifiedAtUtc])
        VALUES
            (@ProcessId, @TenantId, @SiteId, @ProcessName, @Category, 'Draft', 1,
             @OwnerId, @ActorId, @NowUtc, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
