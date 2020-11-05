#region

using System;
using System.Data.Entity.Migrations;

#endregion

namespace RIS.Data.Migrations
{
    public partial class V7_0_0 : DbMigration
    {
        public override void Up()
        {
            //Create tmp tables
            Sql(@"
                CREATE TABLE [tmp_Vehicles] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                , [ViewText] nvarchar(4000) NULL
                , [FaxText] nvarchar(4000) NULL
                , [BosIdentifier] nvarchar(4000) NULL
                , [MainColumn] int NULL
                , [MainRow] int NULL
                , [File] nvarchar(4000) NULL
                );
                GO
                CREATE TABLE [tmp_Users] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                , [MailAdresse] nvarchar(4000) NOT NULL
                , [FaxOn] bit NOT NULL
                );
                GO
                CREATE TABLE [tmp_Printers] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [PrinterName] nvarchar(4000) NOT NULL
                , [FaxCopies] int NOT NULL
                , [FaxNumberOfVehiclesOn] bit NOT NULL
                , [ReportCopies] int NOT NULL
                , [ReportNumberOfVehiclesOn] bit NOT NULL
                , [ReportDistance] int NOT NULL
                , [ReportVehiclesOn] bit NOT NULL
                , [ReportDataOn] bit NOT NULL
                , [ReportRouteImageOn] bit NOT NULL
                , [ReportRouteDescriptionOn] bit NOT NULL
                , [FileprintCopies] int NOT NULL
                , [FileprintNumberOfVehiclesOn] bit NOT NULL
                );
                GO
                CREATE TABLE [tmp_Pagers] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Identifier] nvarchar(4000) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                , [Priority] bit NOT NULL
                , [File] nvarchar(4000) NULL
                );
                GO
                CREATE TABLE [tmp_FilterFields] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                );
                GO
                CREATE TABLE [tmp_Filters] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [SearchExpression] nvarchar(4000) NOT NULL
                , [ReplaceExpression] nvarchar(4000) NULL
                , [DoBeforeShow] bit NOT NULL
                , [Field_Id] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_FileprintConditions] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                );
                GO
                CREATE TABLE [tmp_Fileprints] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                , [Expression] nvarchar(4000) NOT NULL
                , [File] nvarchar(4000) NOT NULL
                , [Condition_Id] int NOT NULL
                , [Printer_Id] int NULL
                );
                GO
                CREATE TABLE [tmp_Ams] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                );
                GO
                CREATE TABLE [tmp_AmsUsers] (
                  [AmsId] int NOT NULL
                , [UserId] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_AmsPagers] (
                  [AmsId] int NOT NULL
                , [PagerId] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_Alarmappgroups] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [GroupId] nvarchar(4000) NOT NULL
                , [GroupName] nvarchar(4000) NOT NULL
                , [FaxOn] bit NOT NULL
                );
                GO
                CREATE TABLE [tmp_AlarmappgroupVehicles] (
                  [AlarmappgroupId] int NOT NULL
                , [VehicleId] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_AlarmappgroupPagers] (
                  [AlarmappgroupId] int NOT NULL
                , [PagerId] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_AaoConditions] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                );
                GO
                CREATE TABLE [tmp_Aaos] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Name] nvarchar(4000) NOT NULL
                , [Expression] nvarchar(4000) NOT NULL
                , [Combination_Id] int NULL
                , [Condition_Id] int NOT NULL
                );
                GO
                CREATE TABLE [tmp_AaoVehicles] (
                  [Id] int IDENTITY (1,1) NOT NULL
                , [Position] int NOT NULL
                , [Aao_Id] int NOT NULL
                , [Vehicle_Id] int NOT NULL
                );    
                GO");

            //Copy data to tmp tables
            Sql(@"                     
                SET IDENTITY_INSERT [tmp_Vehicles] ON;
                GO
                INSERT INTO [tmp_Vehicles]
                SELECT [ID], [Name], [MainText], [FaxText], [FmsKennung], [MainColumn], [MainRow], [Application]
                FROM [Vehicles];  
                GO                        
                SET IDENTITY_INSERT [tmp_Vehicles] OFF;
                GO

                SET IDENTITY_INSERT [tmp_Users] ON;
                GO
                INSERT INTO [tmp_Users]
                SELECT *
                FROM [Users];  
                GO                        
                SET IDENTITY_INSERT [tmp_Users] OFF;
                GO
       
                SET IDENTITY_INSERT [tmp_Printers] ON;
                GO
                INSERT INTO [tmp_Printers]
                SELECT *
                FROM [Printers];  
                GO                        
                SET IDENTITY_INSERT [tmp_Printers] OFF;
                GO
        
                SET IDENTITY_INSERT [tmp_Pagers] ON;
                GO
                INSERT INTO [tmp_Pagers]
                SELECT *
                FROM [ZVEIs];  
                GO                        
                SET IDENTITY_INSERT [tmp_Pagers] OFF;
                GO       

                SET IDENTITY_INSERT [tmp_FilterFields] ON;
                GO
                INSERT INTO [tmp_FilterFields]
                SELECT *
                FROM [FilterFields];  
                GO                        
                SET IDENTITY_INSERT [tmp_FilterFields] OFF;
                GO
      
                SET IDENTITY_INSERT [tmp_Filters] ON;
                GO
                INSERT INTO [tmp_Filters]
                SELECT [ID], [Expression1], [Expression2], [DoBeforeShow], [Field_ID]
                FROM [Filters];  
                GO                        
                SET IDENTITY_INSERT [tmp_Filters] OFF;
                GO

                SET IDENTITY_INSERT [tmp_FileprintConditions] ON;
                GO
                INSERT INTO [tmp_FileprintConditions]
                SELECT *
                FROM [FilePrintConditions];  
                GO                        
                SET IDENTITY_INSERT [tmp_FileprintConditions] OFF;
                GO
    
                SET IDENTITY_INSERT [tmp_Fileprints] ON;
                GO
                INSERT INTO [tmp_Fileprints]
                SELECT *
                FROM [FilePrints];  
                GO                        
                SET IDENTITY_INSERT [tmp_Fileprints] OFF;
                GO
 
                SET IDENTITY_INSERT [tmp_Ams] ON;
                GO
                INSERT INTO [tmp_Ams]
                SELECT *
                FROM [AMS];  
                GO                        
                SET IDENTITY_INSERT [tmp_Ams] OFF;
                GO
     
                INSERT INTO [tmp_AmsUsers]
                SELECT [AMS_ID], [User_ID]
                FROM [UserAMS];  
                GO       

                INSERT INTO [tmp_AmsPagers]
                SELECT [AMS_ID], [ZVEI_ID]
                FROM [AMSZVEIs];  
                GO   
                    
                SET IDENTITY_INSERT [tmp_Alarmappgroups] ON;
                GO
                INSERT INTO [tmp_Alarmappgroups]
                SELECT *
                FROM [AlarmappGroups];  
                GO                       
                SET IDENTITY_INSERT [tmp_Alarmappgroups] OFF;
                GO   
                      
                INSERT INTO [tmp_AlarmappgroupVehicles]    
                SELECT [AlarmappGroup_ID], [Vehicle_ID]
                FROM [AlarmappGroupVehicles];  
                GO   
                      
                INSERT INTO [tmp_AlarmappgroupPagers]
                SELECT [AlarmappGroup_ID], [ZVEI_ID]
                FROM [ZVEIAlarmappGroups];  
                GO         
                    
                SET IDENTITY_INSERT [tmp_AaoConditions] ON;
                GO
                INSERT INTO [tmp_AaoConditions]
                SELECT *
                FROM [AaoConditions];  
                GO                       
                SET IDENTITY_INSERT [tmp_AaoConditions] OFF;
                GO           
                
                SET IDENTITY_INSERT [tmp_Aaos] ON;
                GO
                INSERT INTO [tmp_Aaos]
                SELECT *
                FROM [AAOs];  
                GO                       
                SET IDENTITY_INSERT [tmp_Aaos] OFF;
                GO      
                   
                SET IDENTITY_INSERT [tmp_AaoVehicles] ON;
                GO
                INSERT INTO [tmp_AaoVehicles]
                SELECT *
                FROM [AaoVehicles] WHERE [Aao_ID] IS NOT NULL AND [Vehicle_ID] IS NOT NULL;  
                GO                       
                SET IDENTITY_INSERT [tmp_AaoVehicles] OFF;
                GO    
                ");

            //Delete old tables
            Sql(@"   
                DROP TABLE [ZVEIAlarmappGroups];
                GO          
                DROP TABLE [AMSZVEIs];
                GO
                DROP TABLE [ZVEIs];  
                GO        
                DROP TABLE [AlarmappGroupVehicles];    
                GO
                DROP TABLE [AaoVehicles];
                GO       
                DROP TABLE [Vehicles];   
                GO        
                DROP TABLE [Filters];              
                GO        
                DROP TABLE [FilterActions];    
                GO 
                DROP TABLE [FilterFields];      
                GO     
                DROP TABLE [FilePrints];     
                GO       
                DROP TABLE [FilePrintConditions];    
                GO
                DROP TABLE [Printers];  
                GO      
                DROP TABLE [UserAMS];      
                GO
                DROP TABLE [AMS];         
                GO      
                DROP TABLE [AAOs];  
                GO   
                DROP TABLE [AaoConditions];    
                GO 
                DROP TABLE [AlarmappGroups];        
                GO     
                DROP TABLE [Users];     
                GO   
                ");


            //Rename tmp tables
            Sql(@" 
                sp_rename 'tmp_Vehicles', 'Vehicles';       
                GO   
                sp_rename 'tmp_Users', 'Users';       
                GO   
                sp_rename 'tmp_Printers', 'Printers';      
                GO   
                sp_rename 'tmp_Pagers', 'Pagers';        
                GO   
                sp_rename 'tmp_FilterFields', 'FilterFields';        
                GO   
                sp_rename 'tmp_Filters', 'Filters';     
                GO   
                sp_rename 'tmp_FileprintConditions', 'FileprintConditions';      
                GO   
                sp_rename 'tmp_Fileprints', 'Fileprints';       
                GO   
                sp_rename 'tmp_Ams', 'Ams';             
                GO   
                sp_rename 'tmp_AmsUsers', 'AmsUsers';       
                GO   
                sp_rename 'tmp_AmsPagers', 'AmsPagers';       
                GO   
                sp_rename 'tmp_Alarmappgroups', 'Alarmappgroups';      
                GO   
                sp_rename 'tmp_AlarmappgroupVehicles', 'AlarmappgroupVehicles';      
                GO   
                sp_rename 'tmp_AlarmappgroupPagers', 'AlarmappgroupPagers';      
                GO   
                sp_rename 'tmp_AaoConditions', 'AaoConditions';        
                GO   
                sp_rename 'tmp_Aaos', 'Aaos';         
                GO   
                sp_rename 'tmp_AaoVehicles', 'AaoVehicles';     
                GO     
                ");

            //Create Index and constraints
            Sql(@" 
                ALTER TABLE [Vehicles] ADD CONSTRAINT [PK_dbo.Vehicles] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Users] ADD CONSTRAINT [PK_dbo.Users] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Printers] ADD CONSTRAINT [PK_dbo.Printers] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Pagers] ADD CONSTRAINT [PK_dbo.Pagers] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [FilterFields] ADD CONSTRAINT [PK_dbo.FilterFields] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Filters] ADD CONSTRAINT [PK_dbo.Filters] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [FileprintConditions] ADD CONSTRAINT [PK_dbo.FileprintConditions] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Fileprints] ADD CONSTRAINT [PK_dbo.Fileprints] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Ams] ADD CONSTRAINT [PK_dbo.Ams] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [AmsUsers] ADD CONSTRAINT [PK_dbo.AmsUsers] PRIMARY KEY ([AmsId],[UserId]);
                GO
                ALTER TABLE [AmsPagers] ADD CONSTRAINT [PK_dbo.AmsPagers] PRIMARY KEY ([AmsId],[PagerId]);
                GO
                ALTER TABLE [Alarmappgroups] ADD CONSTRAINT [PK_dbo.Alarmappgroups] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [AlarmappgroupVehicles] ADD CONSTRAINT [PK_dbo.AlarmappgroupVehicles] PRIMARY KEY ([AlarmappgroupId],[VehicleId]);
                GO
                ALTER TABLE [AlarmappgroupPagers] ADD CONSTRAINT [PK_dbo.AlarmappgroupPagers] PRIMARY KEY ([AlarmappgroupId],[PagerId]);
                GO
                ALTER TABLE [AaoConditions] ADD CONSTRAINT [PK_dbo.AaoConditions] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [Aaos] ADD CONSTRAINT [PK_dbo.Aaos] PRIMARY KEY ([Id]);
                GO
                ALTER TABLE [AaoVehicles] ADD CONSTRAINT [PK_dbo.AaoVehicles] PRIMARY KEY ([Id]);
                GO
                CREATE INDEX [IX_Field_Id] ON [Filters] ([Field_Id] ASC);
                GO
                CREATE INDEX [IX_Condition_Id] ON [Fileprints] ([Condition_Id] ASC);
                GO
                CREATE INDEX [IX_Printer_Id] ON [Fileprints] ([Printer_Id] ASC);
                GO
                CREATE INDEX [IX_AmsId] ON [AmsUsers] ([AmsId] ASC);
                GO
                CREATE INDEX [IX_UserId] ON [AmsUsers] ([UserId] ASC);
                GO
                CREATE INDEX [IX_AmsId] ON [AmsPagers] ([AmsId] ASC);
                GO
                CREATE INDEX [IX_PagerId] ON [AmsPagers] ([PagerId] ASC);
                GO
                CREATE INDEX [IX_AlarmappgroupId] ON [AlarmappgroupVehicles] ([AlarmappgroupId] ASC);
                GO
                CREATE INDEX [IX_VehicleId] ON [AlarmappgroupVehicles] ([VehicleId] ASC);
                GO
                CREATE INDEX [IX_AlarmappgroupId] ON [AlarmappgroupPagers] ([AlarmappgroupId] ASC);
                GO
                CREATE INDEX [IX_PagerId] ON [AlarmappgroupPagers] ([PagerId] ASC);
                GO
                CREATE INDEX [IX_Combination_Id] ON [Aaos] ([Combination_Id] ASC);
                GO
                CREATE INDEX [IX_Condition_Id] ON [Aaos] ([Condition_Id] ASC);
                GO
                CREATE INDEX [IX_Aao_Id] ON [AaoVehicles] ([Aao_Id] ASC);
                GO
                CREATE INDEX [IX_Vehicle_Id] ON [AaoVehicles] ([Vehicle_Id] ASC);
                GO
                ALTER TABLE [Filters] ADD CONSTRAINT [FK_dbo.Filters_dbo.FilterFields_Field_Id] FOREIGN KEY ([Field_Id]) REFERENCES [FilterFields]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [Fileprints] ADD CONSTRAINT [FK_dbo.Fileprints_dbo.FileprintConditions_Condition_Id] FOREIGN KEY ([Condition_Id]) REFERENCES [FileprintConditions]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [Fileprints] ADD CONSTRAINT [FK_dbo.Fileprints_dbo.Printers_Printer_Id] FOREIGN KEY ([Printer_Id]) REFERENCES [Printers]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AmsUsers] ADD CONSTRAINT [FK_dbo.AmsUsers_dbo.Ams_AmsId] FOREIGN KEY ([AmsId]) REFERENCES [Ams]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AmsUsers] ADD CONSTRAINT [FK_dbo.AmsUsers_dbo.Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AmsPagers] ADD CONSTRAINT [FK_dbo.AmsPagers_dbo.Ams_AmsId] FOREIGN KEY ([AmsId]) REFERENCES [Ams]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AmsPagers] ADD CONSTRAINT [FK_dbo.AmsPagers_dbo.Pagers_PagerId] FOREIGN KEY ([PagerId]) REFERENCES [Pagers]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AlarmappgroupVehicles] ADD CONSTRAINT [FK_dbo.AlarmappgroupVehicles_dbo.Alarmappgroups_AlarmappgroupId] FOREIGN KEY ([AlarmappgroupId]) REFERENCES [Alarmappgroups]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AlarmappgroupVehicles] ADD CONSTRAINT [FK_dbo.AlarmappgroupVehicles_dbo.Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AlarmappgroupPagers] ADD CONSTRAINT [FK_dbo.AlarmappgroupPagers_dbo.Alarmappgroups_AlarmappgroupId] FOREIGN KEY ([AlarmappgroupId]) REFERENCES [Alarmappgroups]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AlarmappgroupPagers] ADD CONSTRAINT [FK_dbo.AlarmappgroupPagers_dbo.Pagers_PagerId] FOREIGN KEY ([PagerId]) REFERENCES [Pagers]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [Aaos] ADD CONSTRAINT [FK_dbo.Aaos_dbo.AaoConditions_Condition_Id] FOREIGN KEY ([Condition_Id]) REFERENCES [AaoConditions]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [Aaos] ADD CONSTRAINT [FK_dbo.Aaos_dbo.Aaos_Combination_Id] FOREIGN KEY ([Combination_Id]) REFERENCES [Aaos]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AaoVehicles] ADD CONSTRAINT [FK_dbo.AaoVehicles_dbo.Aaos_Aao_Id] FOREIGN KEY ([Aao_Id]) REFERENCES [Aaos]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO
                ALTER TABLE [AaoVehicles] ADD CONSTRAINT [FK_dbo.AaoVehicles_dbo.Vehicles_Vehicle_Id] FOREIGN KEY ([Vehicle_Id]) REFERENCES [Vehicles]([Id]) ON DELETE CASCADE ON UPDATE NO ACTION;
                GO 
                ");
        }

        public override void Down()
        {
            throw new NotSupportedException("Downgrade of database not possible");
        }
    }
}