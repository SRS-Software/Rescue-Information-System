#region

using System.Data.Entity.Migrations;

#endregion

namespace RIS.Data.Migrations
{
    public partial class V7_0_1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "FaxMessageService_FaxOn", c => c.Boolean(false));
            Sql(@"UPDATE [Users] SET FaxMessageService_FaxOn = FaxOn");
            DropColumn("dbo.Users", "FaxOn");
            AddColumn("dbo.Users", "FaxMessageService_MailOn", c => c.Boolean(false));
            AddColumn("dbo.Users", "AlarmMessageService_RecordOn", c => c.Boolean(false));
        }

        public override void Down()
        {
            AddColumn("dbo.Users", "FaxOn", c => c.Boolean(false));
            Sql(@"UPDATE [Users] SET FaxOn = FaxMessageService_FaxOn");
            DropColumn("dbo.Users", "FaxMessageService_FaxOn");
            DropColumn("dbo.Users", "FaxMessageService_MailOn");
            DropColumn("dbo.Users", "AlarmMessageService_RecordOn");
        }
    }
}