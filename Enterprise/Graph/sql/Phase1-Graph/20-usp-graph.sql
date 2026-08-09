-- =============================================
-- Enterprise.Graph -- Phase1-Graph
-- Script:        20-usp-graph.sql
-- Purpose:       usp_GraphNode_Add, usp_GraphEdge_Add,
--                usp_GraphNode_GetNeighbors (one hop, either direction).
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EnterpriseGraph].[usp_GraphNode_Add]
    @NodeId        uniqueidentifier,
    @NodeType      nvarchar(20),
    @SourceId      uniqueidentifier,
    @Label         nvarchar(400),
    @CreatedAtUtc  datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @NodeId IS NULL OR @SourceId IS NULL
            THROW 50001, N'NodeId and SourceId are required.', 1;

        INSERT INTO [EnterpriseGraph].[GraphNode] ([NodeId], [NodeType], [SourceId], [Label], [CreatedAtUtc])
        VALUES (@NodeId, @NodeType, @SourceId, @Label, @CreatedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext90 nvarchar(200) = CONCAT(N'Params=NodeId=', @NodeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails90 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext90, @ErrorDetails90, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseGraph].[usp_GraphEdge_Add]
    @FromNodeId    uniqueidentifier,
    @ToNodeId      uniqueidentifier,
    @EdgeType      nvarchar(20),
    @CreatedAtUtc  datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FromNodeId IS NULL OR @ToNodeId IS NULL
            THROW 50001, N'FromNodeId and ToNodeId are required.', 1;

        INSERT INTO [EnterpriseGraph].[GraphEdge] ([FromNodeId], [ToNodeId], [EdgeType], [CreatedAtUtc])
        VALUES (@FromNodeId, @ToNodeId, @EdgeType, @CreatedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext91 nvarchar(200) = CONCAT(N'Params=FromNodeId=', @FromNodeId, N'; ToNodeId=', @ToNodeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails91 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext91, @ErrorDetails91, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseGraph].[usp_GraphNode_GetNeighbors]
    @NodeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @NodeId IS NULL
            THROW 50001, N'NodeId is required.', 1;

        SELECT e.[GraphEdgeId], e.[FromNodeId], e.[ToNodeId], e.[EdgeType],
               n.[NodeId] AS [NeighborNodeId], n.[NodeType] AS [NeighborNodeType], n.[Label] AS [NeighborLabel]
        FROM [EnterpriseGraph].[GraphEdge] AS e
        JOIN [EnterpriseGraph].[GraphNode] AS n
            ON n.[NodeId] = CASE WHEN e.[FromNodeId] = @NodeId THEN e.[ToNodeId] ELSE e.[FromNodeId] END
        WHERE (e.[FromNodeId] = @NodeId OR e.[ToNodeId] = @NodeId)
          AND ISNULL(e.[IsDeleted], 0) = 0 AND ISNULL(n.[IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext92 nvarchar(200) = CONCAT(N'Params=NodeId=', @NodeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails92 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext92, @ErrorDetails92, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
