namespace ThunderITforGEA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "login", c => c.String());
            AddColumn("dbo.AspNetUsers", "imie", c => c.String());
            AddColumn("dbo.AspNetUsers", "nazwisko", c => c.String());
            AddColumn("dbo.AspNetUsers", "firma", c => c.String());
            AddColumn("dbo.AspNetUsers", "adres", c => c.String());
            AddColumn("dbo.AspNetUsers", "miasto", c => c.String());
            AddColumn("dbo.AspNetUsers", "kraj", c => c.String());
            AddColumn("dbo.AspNetUsers", "telefon", c => c.String());
            AddColumn("dbo.AspNetUsers", "mail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "mail");
            DropColumn("dbo.AspNetUsers", "telefon");
            DropColumn("dbo.AspNetUsers", "kraj");
            DropColumn("dbo.AspNetUsers", "miasto");
            DropColumn("dbo.AspNetUsers", "adres");
            DropColumn("dbo.AspNetUsers", "firma");
            DropColumn("dbo.AspNetUsers", "nazwisko");
            DropColumn("dbo.AspNetUsers", "imie");
            DropColumn("dbo.AspNetUsers", "login");
        }
    }
}
