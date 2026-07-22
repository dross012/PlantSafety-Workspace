-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.HazardCommunicationRightToKnow
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationRightToKnow_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[HazardCommunicationRightToKnow], backing
--                  IHazardCommunicationRightToKnowRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_HazardCommunicationRightToKnow_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @HazardCommunicationRightToKnowId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @HazardClass NVARCHAR(100),
    @SdsReference NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextReviewDueAtUtc DATETIME2(3) = NULL,
    @LabeledItemCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyProcedures].[HazardCommunicationRightToKnow]
            ([HazardCommunicationRightToKnowId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HazardClass], [SdsReference], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [LabeledItemCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@HazardCommunicationRightToKnowId, @TenantId, @SiteId, @Status, @ProcedureTitle, @HazardClass, @SdsReference, @ApprovedByUserId, @PublishedAtUtc, @NextReviewDueAtUtc, @LabeledItemCount, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
