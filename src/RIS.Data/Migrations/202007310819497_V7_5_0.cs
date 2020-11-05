#region

using System.Data.Entity.Migrations;

#endregion

namespace RIS.Data.Migrations
{
    public partial class V7_5_0 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AlarmappGroups", "OnlyWithPager", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.AlarmappGroups", "OnlyWithPager");
        }
    }
}