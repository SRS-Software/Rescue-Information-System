#region

using System.Data.Entity.Migrations;

#endregion

namespace RIS.Data.Migrations
{
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.AaoConditions", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.AAOs", c => new
                {
                    ID = c.Int(false, true),
                    Name = c.String(maxLength: 4000),
                    Expression = c.String(maxLength: 4000),
                    Combination_ID = c.Int(),
                    Condition_ID = c.Int()
                }).PrimaryKey(t => t.ID).ForeignKey("dbo.AAOs", t => t.Combination_ID)
                .ForeignKey("dbo.AaoConditions", t => t.Condition_ID).Index(t => t.Combination_ID)
                .Index(t => t.Condition_ID);

            CreateTable("dbo.AaoVehicles", c => new
                {
                    ID = c.Int(false, true),
                    Position = c.Int(false),
                    Aao_ID = c.Int(),
                    Vehicle_ID = c.Int()
                }).PrimaryKey(t => t.ID).ForeignKey("dbo.AAOs", t => t.Aao_ID)
                .ForeignKey("dbo.Vehicles", t => t.Vehicle_ID)
                .Index(t => t.Aao_ID).Index(t => t.Vehicle_ID);

            CreateTable("dbo.Vehicles", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000),
                FmsKennung = c.String(maxLength: 4000),
                MainOn = c.Boolean(false),
                MainText = c.String(maxLength: 4000),
                MainColumn = c.Int(),
                MainRow = c.Int(),
                FaxOn = c.Boolean(false),
                FaxText = c.String(maxLength: 4000),
                Application = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.AlarmappGroups", c => new
            {
                ID = c.Int(false, true),
                GroupId = c.String(maxLength: 4000),
                Name = c.String(maxLength: 4000),
                FaxOn = c.Boolean(false)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.ZVEIs", c => new
            {
                ID = c.Int(false, true),
                Kennung = c.String(maxLength: 4000),
                Bezeichnung = c.String(maxLength: 4000),
                Priority = c.Boolean(false),
                Sound = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.AMS", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.Users", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000),
                MailAdresse = c.String(maxLength: 4000),
                FaxOn = c.Boolean(false)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.FilePrintConditions", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.FilePrints", c => new
                {
                    ID = c.Int(false, true),
                    Name = c.String(maxLength: 4000),
                    Expression = c.String(maxLength: 4000),
                    File = c.String(maxLength: 4000),
                    Condition_ID = c.Int(),
                    Printer_ID = c.Int()
                }).PrimaryKey(t => t.ID).ForeignKey("dbo.FilePrintConditions", t => t.Condition_ID)
                .ForeignKey("dbo.Printers", t => t.Printer_ID).Index(t => t.Condition_ID).Index(t => t.Printer_ID);

            CreateTable("dbo.Printers", c => new
            {
                ID = c.Int(false, true),
                PrinterName = c.String(maxLength: 4000),
                FaxCopies = c.Int(false),
                FaxNumberOfVehiclesOn = c.Boolean(false),
                ReportCopies = c.Int(false),
                ReportNumberOfVehiclesOn = c.Boolean(false),
                ReportDistance = c.Int(false),
                ReportVehiclesOn = c.Boolean(false),
                ReportDataOn = c.Boolean(false),
                ReportRouteImageOn = c.Boolean(false),
                ReportRouteDescriptionOn = c.Boolean(false),
                FilePrintCopies = c.Int(false),
                FilePrintNumberOfVehiclesOn = c.Boolean(false)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.FilterActions", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.Filters", c => new
                {
                    ID = c.Int(false, true),
                    Expression1 = c.String(maxLength: 4000),
                    Expression2 = c.String(maxLength: 4000),
                    DoBeforeShow = c.Boolean(false),
                    Action_ID = c.Int(),
                    Field_ID = c.Int()
                }).PrimaryKey(t => t.ID).ForeignKey("dbo.FilterActions", t => t.Action_ID)
                .ForeignKey("dbo.FilterFields", t => t.Field_ID).Index(t => t.Action_ID).Index(t => t.Field_ID);

            CreateTable("dbo.FilterFields", c => new
            {
                ID = c.Int(false, true),
                Name = c.String(maxLength: 4000)
            }).PrimaryKey(t => t.ID);

            CreateTable("dbo.AlarmappGroupVehicles", c => new
                {
                    AlarmappGroup_ID = c.Int(false),
                    Vehicle_ID = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.AlarmappGroup_ID,
                    t.Vehicle_ID
                }).ForeignKey("dbo.AlarmappGroups", t => t.AlarmappGroup_ID, true)
                .ForeignKey("dbo.Vehicles", t => t.Vehicle_ID, true).Index(t => t.AlarmappGroup_ID)
                .Index(t => t.Vehicle_ID);

            CreateTable("dbo.ZVEIAlarmappGroups", c => new
                {
                    ZVEI_ID = c.Int(false),
                    AlarmappGroup_ID = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.ZVEI_ID,
                    t.AlarmappGroup_ID
                }).ForeignKey("dbo.ZVEIs", t => t.ZVEI_ID, true)
                .ForeignKey("dbo.AlarmappGroups", t => t.AlarmappGroup_ID, true).Index(t => t.ZVEI_ID)
                .Index(t => t.AlarmappGroup_ID);

            CreateTable("dbo.UserAMS", c => new
                {
                    User_ID = c.Int(false),
                    AMS_ID = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.User_ID,
                    t.AMS_ID
                }).ForeignKey("dbo.Users", t => t.User_ID, true).ForeignKey("dbo.AMS", t => t.AMS_ID, true)
                .Index(t => t.User_ID).Index(t => t.AMS_ID);

            CreateTable("dbo.AMSZVEIs", c => new
                {
                    AMS_ID = c.Int(false),
                    ZVEI_ID = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.AMS_ID,
                    t.ZVEI_ID
                }).ForeignKey("dbo.AMS", t => t.AMS_ID, true).ForeignKey("dbo.ZVEIs", t => t.ZVEI_ID, true)
                .Index(t => t.AMS_ID).Index(t => t.ZVEI_ID);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Filters", "Field_ID", "dbo.FilterFields");
            DropForeignKey("dbo.Filters", "Action_ID", "dbo.FilterActions");
            DropForeignKey("dbo.FilePrints", "Printer_ID", "dbo.Printers");
            DropForeignKey("dbo.FilePrints", "Condition_ID", "dbo.FilePrintConditions");
            DropForeignKey("dbo.AMSZVEIs", "ZVEI_ID", "dbo.ZVEIs");
            DropForeignKey("dbo.AMSZVEIs", "AMS_ID", "dbo.AMS");
            DropForeignKey("dbo.UserAMS", "AMS_ID", "dbo.AMS");
            DropForeignKey("dbo.UserAMS", "User_ID", "dbo.Users");
            DropForeignKey("dbo.ZVEIAlarmappGroups", "AlarmappGroup_ID", "dbo.AlarmappGroups");
            DropForeignKey("dbo.ZVEIAlarmappGroups", "ZVEI_ID", "dbo.ZVEIs");
            DropForeignKey("dbo.AlarmappGroupVehicles", "Vehicle_ID", "dbo.Vehicles");
            DropForeignKey("dbo.AlarmappGroupVehicles", "AlarmappGroup_ID", "dbo.AlarmappGroups");
            DropForeignKey("dbo.AaoVehicles", "Vehicle_ID", "dbo.Vehicles");
            DropForeignKey("dbo.AaoVehicles", "Aao_ID", "dbo.AAOs");
            DropForeignKey("dbo.AAOs", "Condition_ID", "dbo.AaoConditions");
            DropForeignKey("dbo.AAOs", "Combination_ID", "dbo.AAOs");
            DropIndex("dbo.AMSZVEIs", new[]
            {
                "ZVEI_ID"
            });
            DropIndex("dbo.AMSZVEIs", new[]
            {
                "AMS_ID"
            });
            DropIndex("dbo.UserAMS", new[]
            {
                "AMS_ID"
            });
            DropIndex("dbo.UserAMS", new[]
            {
                "User_ID"
            });
            DropIndex("dbo.ZVEIAlarmappGroups", new[]
            {
                "AlarmappGroup_ID"
            });
            DropIndex("dbo.ZVEIAlarmappGroups", new[]
            {
                "ZVEI_ID"
            });
            DropIndex("dbo.AlarmappGroupVehicles", new[]
            {
                "Vehicle_ID"
            });
            DropIndex("dbo.AlarmappGroupVehicles", new[]
            {
                "AlarmappGroup_ID"
            });
            DropIndex("dbo.Filters", new[]
            {
                "Field_ID"
            });
            DropIndex("dbo.Filters", new[]
            {
                "Action_ID"
            });
            DropIndex("dbo.FilePrints", new[]
            {
                "Printer_ID"
            });
            DropIndex("dbo.FilePrints", new[]
            {
                "Condition_ID"
            });
            DropIndex("dbo.AaoVehicles", new[]
            {
                "Vehicle_ID"
            });
            DropIndex("dbo.AaoVehicles", new[]
            {
                "Aao_ID"
            });
            DropIndex("dbo.AAOs", new[]
            {
                "Condition_ID"
            });
            DropIndex("dbo.AAOs", new[]
            {
                "Combination_ID"
            });
            DropTable("dbo.AMSZVEIs");
            DropTable("dbo.UserAMS");
            DropTable("dbo.ZVEIAlarmappGroups");
            DropTable("dbo.AlarmappGroupVehicles");
            DropTable("dbo.FilterFields");
            DropTable("dbo.Filters");
            DropTable("dbo.FilterActions");
            DropTable("dbo.Printers");
            DropTable("dbo.FilePrints");
            DropTable("dbo.FilePrintConditions");
            DropTable("dbo.Users");
            DropTable("dbo.AMS");
            DropTable("dbo.ZVEIs");
            DropTable("dbo.AlarmappGroups");
            DropTable("dbo.Vehicles");
            DropTable("dbo.AaoVehicles");
            DropTable("dbo.AAOs");
            DropTable("dbo.AaoConditions");
        }
    }
}