namespace ThunderITforGEA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixEmail : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "mail");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "mail", c => c.String());
        }
    }
}
