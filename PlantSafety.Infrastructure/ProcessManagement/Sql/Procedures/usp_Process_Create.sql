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
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Processes', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProcessId=', @ProcessId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProcessId=', @ProcessId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

    THROW;
END CATCH;
GO
