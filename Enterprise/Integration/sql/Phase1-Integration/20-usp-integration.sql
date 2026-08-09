-- =============================================
-- Enterprise.Integration -- Phase1-Integration
-- Script:        20-usp-integration.sql
-- Purpose:       usp_FieldMapping_Upsert, usp_FieldMapping_GetBySourceSystem,
--                usp_IntegrationEventLog_Record (idempotent on EventId,
--                mirrors Phase3-Orchestration's precedent for
--                idempotent-no-op-on-duplicate-id event stores).
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EnterpriseIntegration].[usp_FieldMapping_Upsert]
    @SourceSystem       nvarchar(10),
    @ExternalFieldName  nvarchar(200),
    @InternalFieldName  nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceSystem IS NULL OR @ExternalFieldName IS NULL OR @InternalFieldName IS NULL
            THROW 50001, N'SourceSystem, ExternalFieldName, and InternalFieldName are all required.', 1;

        MERGE [EnterpriseIntegration].[FieldMapping] AS target
        USING (SELECT @SourceSystem AS SourceSystem, @ExternalFieldName AS ExternalFieldName) AS source
            ON target.[SourceSystem] = source.SourceSystem AND target.[ExternalFieldName] = source.ExternalFieldName
        WHEN MATCHED THEN
            UPDATE SET [InternalFieldName] = @InternalFieldName, [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([SourceSystem], [ExternalFieldName], [InternalFieldName])
            VALUES (@SourceSystem, @ExternalFieldName, @InternalFieldName);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextF0 nvarchar(200) = CONCAT(N'Params=SourceSystem=', @SourceSystem, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsF0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextF0, @ErrorDetailsF0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseIntegration].[usp_FieldMapping_GetBySourceSystem]
    @SourceSystem nvarchar(10)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT [FieldMappingId], [SourceSystem], [ExternalFieldName], [InternalFieldName]
        FROM [EnterpriseIntegration].[FieldMapping]
        WHERE [SourceSystem] = @SourceSystem AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextF1 nvarchar(200) = CONCAT(N'Params=SourceSystem=', @SourceSystem, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsF1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextF1, @ErrorDetailsF1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseIntegration].[usp_IntegrationEventLog_Record]
    @EventId             uniqueidentifier,
    @SourceSystem        nvarchar(10),
    @EventType           nvarchar(200),
    @MappedPayloadJson   nvarchar(max),
    @OccurredAtUtc       datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EventId IS NULL OR @EventId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'EventId is required and must not be empty.', 1;
        IF ISJSON(@MappedPayloadJson) = 0
            THROW 50001, N'MappedPayloadJson must be valid JSON.', 1;

        IF EXISTS (SELECT 1 FROM [EnterpriseIntegration].[IntegrationEventLog] WHERE [EventId] = @EventId)
            RETURN; -- idempotent no-op on duplicate EventId

        INSERT INTO [EnterpriseIntegration].[IntegrationEventLog] ([EventId], [SourceSystem], [EventType], [MappedPayloadJson], [OccurredAtUtc])
        VALUES (@EventId, @SourceSystem, @EventType, @MappedPayloadJson, @OccurredAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextF2 nvarchar(200) = CONCAT(N'Params=EventId=', @EventId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsF2 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextF2, @ErrorDetailsF2, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
