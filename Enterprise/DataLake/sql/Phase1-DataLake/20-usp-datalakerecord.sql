-- =============================================
-- Enterprise.DataLake -- Phase1-DataLake
-- Script:        20-usp-datalakerecord.sql
-- Purpose:       usp_DataLakeRecord_Append (INSERT-only, no update/replace
--                path -- append-only), usp_DataLakeRecord_GetByPartitionKey,
--                usp_DataLakeRecord_GetByTypeAndTimeRange.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [DataLake].[usp_DataLakeRecord_Append]
    @RecordId       uniqueidentifier,
    @RecordType     nvarchar(20),
    @PartitionKey   nvarchar(200),
    @SourceId       uniqueidentifier,
    @Payload        nvarchar(max),
    @RecordedAtUtc  datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RecordId IS NULL OR @RecordId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'RecordId is required and must not be empty.', 1;
        IF @Payload IS NULL
            THROW 50001, N'Payload is required.', 1;

        IF EXISTS (SELECT 1 FROM [DataLake].[DataLakeRecord] WHERE [RecordId] = @RecordId)
            RETURN; -- idempotent no-op on duplicate RecordId, mirrors the C# store's own semantics

        INSERT INTO [DataLake].[DataLakeRecord] ([RecordId], [RecordType], [PartitionKey], [SourceId], [Payload], [RecordedAtUtc])
        VALUES (@RecordId, @RecordType, @PartitionKey, @SourceId, @Payload, @RecordedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextA0 nvarchar(200) = CONCAT(N'Params=RecordId=', @RecordId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsA0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextA0, @ErrorDetailsA0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [DataLake].[usp_DataLakeRecord_GetByPartitionKey]
    @PartitionKey nvarchar(200),
    @TopN         int = 500
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PartitionKey IS NULL
            THROW 50001, N'PartitionKey is required.', 1;

        SELECT TOP (@TopN) [RecordId], [RecordType], [PartitionKey], [SourceId], [Payload], [RecordedAtUtc]
        FROM [DataLake].[DataLakeRecord]
        WHERE [PartitionKey] = @PartitionKey
        ORDER BY [RecordedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextA1 nvarchar(200) = CONCAT(N'Params=PartitionKey=', @PartitionKey, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsA1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextA1, @ErrorDetailsA1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [DataLake].[usp_DataLakeRecord_GetByTypeAndTimeRange]
    @RecordType     nvarchar(20),
    @FromUtc        datetime2(3),
    @ToUtc          datetime2(3),
    @TopN           int = 5000
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RecordType IS NULL OR @FromUtc IS NULL OR @ToUtc IS NULL
            THROW 50001, N'RecordType, FromUtc, and ToUtc are required.', 1;

        SELECT TOP (@TopN) [RecordId], [RecordType], [PartitionKey], [SourceId], [Payload], [RecordedAtUtc]
        FROM [DataLake].[DataLakeRecord]
        WHERE [RecordType] = @RecordType AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
        ORDER BY [RecordedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextA2 nvarchar(200) = CONCAT(N'Params=RecordType=', @RecordType, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsA2 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextA2, @ErrorDetailsA2, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
