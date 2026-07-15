-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_FabricNode_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Registers a new integration fabric node. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_FabricNode_Create]
(
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @NodeId        UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @NodeName      NVARCHAR(200),
    @NodeType      NVARCHAR(50),
    @Region        NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [EnterpriseFabric].[FabricNode]
            ([NodeId], [TenantId], [SiteId], [NodeName], [NodeType], [Status],
             [LastHeartbeatAtUtc], [Region], [CreatedAtUtc], [CorrelationId], [ActorId])
        VALUES
            (@NodeId, @TenantId, @SiteId, @NodeName, @NodeType, 'Configuring',
             @NowUtc, @Region, @NowUtc, @CorrelationId, @ActorId);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
