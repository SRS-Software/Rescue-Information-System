#region

using System.Data.Entity.Migrations;

#endregion

namespace RIS.Data.Migrations
{
    public partial class V7_2_0 : DbMigration
    {
        public override void Up()
        {
            //Must drop all tables because delete not possible with SQLCE and also migration of data
            DropTable("AlarmappgroupPagers");
            DropTable("AlarmappgroupVehicles");
            DropTable("Alarmappgroups");

            CreateTable("dbo.AlarmappDepartments", c => new
            {
                Id = c.Int(false, true),
                DepartmentId = c.String(false, 4000),
                DepartmentName = c.String(false, 4000)
            }).PrimaryKey(t => t.Id).Index(t => t.Id);

            CreateTable("dbo.AlarmappGroups", c => new
                {
                    Id = c.Int(false, true),
                    GroupId = c.String(maxLength: 4000),
                    GroupName = c.String(maxLength: 4000),
                    FaxOn = c.Boolean(false),
                    Department_Id = c.Int(false)
                }).PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AlarmappDepartments", t => t.Department_Id, true)
                .Index(t => t.Department_Id);

            CreateTable("dbo.AlarmappgroupVehicles", c => new
                {
                    AlarmappgroupId = c.Int(false),
                    VehicleId = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.AlarmappgroupId,
                    t.VehicleId
                }).ForeignKey("dbo.AlarmappGroups", t => t.AlarmappgroupId, true)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId, true).Index(t => t.AlarmappgroupId);

            CreateTable("dbo.AlarmappgroupPagers", c => new
                {
                    AlarmappgroupId = c.Int(false),
                    PagerId = c.Int(false)
                }).PrimaryKey(t => new
                {
                    t.AlarmappgroupId,
                    t.PagerId
                }).ForeignKey("dbo.AlarmappGroups", t => t.AlarmappgroupId, true)
                .ForeignKey("dbo.Pagers", t => t.PagerId, true).Index(t => t.AlarmappgroupId);

            AddColumn("dbo.Printers", "ReportDataSchlagwortOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataStichwortOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataOrtOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataStraßeOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataObjektOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataStationOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataKreuzungOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataAbschnittOn", c => c.Boolean(false));
            AddColumn("dbo.Printers", "ReportDataBemerkungOn", c => c.Boolean(false));
            DropColumn("dbo.Printers", "ReportDataOn");

            //BACKUP
            //DropIndex("dbo.AlarmappGroupPagers", new[] { "AlarmappgroupId" });
            //DropIndex("dbo.AlarmappGroupVehicles", new[] { "AlarmappgroupId" });
            //CreateTable(
            //    "dbo.AlarmappDepartments",
            //    c => new
            //    {
            //        Id = c.Int(nullable: false, identity: true),
            //        DepartmentId = c.String(nullable: false, maxLength: 4000),
            //        DepartmentName = c.String(nullable: false, maxLength: 4000),
            //    })
            //    .PrimaryKey(t => t.Id);
            //AddColumn("dbo.AlarmappGroups", "Department_Id", c => c.Int(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataSchlagwortOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataStichwortOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataOrtOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataStraßeOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataObjektOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataStationOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataKreuzungOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataAbschnittOn", c => c.Boolean(nullable: false));
            //AddColumn("dbo.Printers", "ReportDataBemerkungOn", c => c.Boolean(nullable: false));
            //CreateIndex("dbo.AlarmappGroups", "Department_Id");
            //CreateIndex("dbo.AlarmappGroupPagers", "AlarmappGroupId");
            //CreateIndex("dbo.AlarmappGroupVehicles", "AlarmappGroupId");
            //AddForeignKey("dbo.AlarmappGroups", "Department_Id", "dbo.AlarmappDepartments", "Id", cascadeDelete: true);
            //DropColumn("dbo.Printers", "ReportDataOn");
        }

        public override void Down()
        {
            AddColumn("dbo.Printers", "ReportDataOn", c => c.Boolean(false));
            DropForeignKey("dbo.AlarmappGroups", "Department_Id", "dbo.AlarmappDepartments");
            DropIndex("dbo.AlarmappGroupVehicles", new[] {"AlarmappGroupId"});
            DropIndex("dbo.AlarmappGroupPagers", new[] {"AlarmappGroupId"});
            DropIndex("dbo.AlarmappGroups", new[] {"Department_Id"});
            DropColumn("dbo.Printers", "ReportDataBemerkungOn");
            DropColumn("dbo.Printers", "ReportDataAbschnittOn");
            DropColumn("dbo.Printers", "ReportDataKreuzungOn");
            DropColumn("dbo.Printers", "ReportDataStationOn");
            DropColumn("dbo.Printers", "ReportDataObjektOn");
            DropColumn("dbo.Printers", "ReportDataStraßeOn");
            DropColumn("dbo.Printers", "ReportDataOrtOn");
            DropColumn("dbo.Printers", "ReportDataStichwortOn");
            DropColumn("dbo.Printers", "ReportDataSchlagwortOn");
            DropColumn("dbo.AlarmappGroups", "Department_Id");
            DropTable("dbo.AlarmappDepartments");
            CreateIndex("dbo.AlarmappGroupVehicles", "AlarmappgroupId");
            CreateIndex("dbo.AlarmappGroupPagers", "AlarmappgroupId");
        }
    }
}