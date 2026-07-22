-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.LotoProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoProcedures_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[LotoProcedures], backing
--                  ILotoProceduresRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_LotoProcedures_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @LotoProceduresId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @EnergySourceTypes NVARCHAR(100),
    @IsolationPointDescription NVARCHAR(2000),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextAuditDueAtUtc DATETIME2(3) = NULL,
    @AuthorizedPersonnelCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyProcedures].[LotoProcedures]
            ([LotoProceduresId], [TenantId], [SiteId], [Status], [ProcedureTitle], [EnergySourceTypes], [IsolationPointDescription], [ApprovedByUserId], [PublishedAtUtc], [NextAuditDueAtUtc], [AuthorizedPersonnelCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@LotoProceduresId, @TenantId, @SiteId, @Status, @ProcedureTitle, @EnergySourceTypes, @IsolationPointDescription, @ApprovedByUserId, @PublishedAtUtc, @NextAuditDueAtUtc, @AuthorizedPersonnelCount, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
