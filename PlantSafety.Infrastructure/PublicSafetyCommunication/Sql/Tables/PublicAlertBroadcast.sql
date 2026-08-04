-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PublicAlertBroadcast.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PublicAlertBroadcast aggregate root
--                  (IPublicAlertBroadcastRepository). Tenant-scoped. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PublicAlertBroadcast' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
BEGIN
    CREATE TABLE [PublicSafetyCommunication].[PublicAlertBroadcast]
    (
        PublicAlertBroadcastId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PublicAlertBroadcast PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        IncidentId UNIQUEIDENTIFIER NULL,
        SeverityCode NVARCHAR(64) NOT NULL,
        ChannelCode NVARCHAR(64) NOT NULL,
        Headline NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        TargetArea NVARCHAR(400) NOT NULL,
        IssuedAtUtc DATETIME2(3) NULL,
        IssuedBy UNIQUEIDENTIFIER NULL,
        DeliveryConfirmedAtUtc DATETIME2(3) NULL,
        FailureReason NVARCHAR(400) NULL,
        CancelledAtUtc DATETIME2(3) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PublicAlertBroadcast_IsDeleted DEFAULT (0),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_TenantId_Site ON [PublicSafetyCommunication].[PublicAlertBroadcast] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_IncidentId ON [PublicSafetyCommunication].[PublicAlertBroadcast] (IncidentId);
    CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_Status ON [PublicSafetyCommunication].[PublicAlertBroadcast] (Status, IsDeleted);
END
GO
