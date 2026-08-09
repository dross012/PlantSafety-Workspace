-- =============================================
-- Shared.FacilityDomain -- Phase3-Propagation
-- Script:        20-usp-propagation.sql
-- Purpose:       usp_HazardPropagationEvent_Record / GetRecent,
--                usp_HazardChain_Record (chain header + nodes, one
--                transaction) / usp_HazardChain_GetByFacilityId.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Propagation].[usp_HazardPropagationEvent_Record]
    @FacilityId     uniqueidentifier,
    @FromStage      nvarchar(30),
    @ToStage        nvarchar(30),
    @OccurredAtUtc  datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        INSERT INTO [Propagation].[HazardPropagationEvent] ([FacilityId], [FromStage], [ToStage], [OccurredAtUtc])
        VALUES (@FacilityId, @FromStage, @ToStage, @OccurredAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext30 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails30 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext30, @ErrorDetails30, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Propagation].[usp_HazardPropagationEvent_GetRecent]
    @FacilityId  uniqueidentifier,
    @TopN        int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT TOP (@TopN) [HazardPropagationEventId], [FacilityId], [FromStage], [ToStage], [OccurredAtUtc]
        FROM [Propagation].[HazardPropagationEvent]
        WHERE [FacilityId] = @FacilityId AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [OccurredAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext31 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails31 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext31, @ErrorDetails31, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Propagation].[usp_HazardChain_GetByFacilityId]
    @FacilityId  uniqueidentifier,
    @TopN        int = 20
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT c.[HazardChainId], c.[FacilityId], c.[BuiltAtUtc],
               n.[SequenceNumber], n.[Stage], n.[SeverityCode], n.[OccurredAtUtc], n.[Description]
        FROM [Propagation].[HazardChain] AS c
        JOIN [Propagation].[HazardChainNode] AS n ON n.[HazardChainId] = c.[HazardChainId]
        WHERE c.[FacilityId] = @FacilityId AND ISNULL(c.[IsDeleted], 0) = 0
          AND c.[HazardChainId] IN (
              SELECT TOP (@TopN) [HazardChainId] FROM [Propagation].[HazardChain]
              WHERE [FacilityId] = @FacilityId AND ISNULL([IsDeleted], 0) = 0
              ORDER BY [BuiltAtUtc] DESC)
        ORDER BY c.[BuiltAtUtc] DESC, n.[SequenceNumber] ASC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext32 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails32 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext32, @ErrorDetails32, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
