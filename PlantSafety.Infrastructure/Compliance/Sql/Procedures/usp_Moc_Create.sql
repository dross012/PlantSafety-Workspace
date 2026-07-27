-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Moc_Create.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IMocRepository.CreateAsync. Creates a new MOC
--                  in Draft status.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Create]
    @TenantId         UNIQUEIDENTIFIER,
    @CorrelationId    UNIQUEIDENTIFIER,
    @ActorId          UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @MocId             UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @Title             NVARCHAR(200),
    @Description       NVARCHAR(4000),
    @ChangeType        NVARCHAR(50),
    @RiskLevel         NVARCHAR(50),
    @InitiatorId       UNIQUEIDENTIFIER,
    @ProposedStartUtc  DATETIME2(3),
    @ProposedEndUtc    DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        INSERT INTO [Compliance].[Moc]
        (
            MocId, TenantId, SiteId, Title, Description, ChangeType, RiskLevel, InitiatorId,
            ProposedStartUtc, ProposedEndUtc, Status, LastModifiedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @MocId, @TenantId, @SiteId, @Title, @Description, @ChangeType, @RiskLevel, @InitiatorId,
            @ProposedStartUtc, @ProposedEndUtc, N'Draft', @NowUtc, @NowUtc, @ActorId, 0
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
