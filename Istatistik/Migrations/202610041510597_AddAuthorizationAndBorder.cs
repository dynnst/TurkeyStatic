namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorizationAndBorder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bureaux",
                c => new
                    {
                        BureauId = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Code = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BureauId)
                .Index(t => new { t.Border, t.Code }, unique: true, name: "IX_Bureau_Border_Code");
            
            CreateTable(
                "dbo.UserAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Sicil = c.String(nullable: false, maxLength: 50),
                        Border = c.String(nullable: false, maxLength: 150),
                        Role = c.String(nullable: false, maxLength: 30),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Sicil, unique: true)
                .Index(t => t.Border);
            
            CreateTable(
                "dbo.UserBureaux",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAssignmentId = c.Int(nullable: false),
                        BureauId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bureaux", t => t.BureauId, cascadeDelete: true)
                .ForeignKey("dbo.UserAssignments", t => t.UserAssignmentId, cascadeDelete: true)
                .Index(t => new { t.UserAssignmentId, t.BureauId }, unique: true, name: "IX_UserBureau");
            
            AddColumn("dbo.GunlukZamanSerisiYolcus", "Border", c => c.String(nullable: false, maxLength: 150));
            AddColumn("dbo.HaftalikOlayCizelgesis", "Border", c => c.String(nullable: false, maxLength: 150));
            AddColumn("dbo.InadYolcus", "Border", c => c.String(nullable: false, maxLength: 150));
            AddColumn("dbo.TahditKayits", "Border", c => c.String(nullable: false, maxLength: 150));
            AddColumn("dbo.YolcuUcakIstatistiks", "Border", c => c.String(nullable: false, maxLength: 150));
            CreateIndex("dbo.GunlukZamanSerisiYolcus", "Border");
            CreateIndex("dbo.HaftalikOlayCizelgesis", "Border");
            CreateIndex("dbo.InadYolcus", "Border");
            CreateIndex("dbo.TahditKayits", "Border");
            CreateIndex("dbo.YolcuUcakIstatistiks", "Border");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserBureaux", "UserAssignmentId", "dbo.UserAssignments");
            DropForeignKey("dbo.UserBureaux", "BureauId", "dbo.Bureaux");
            DropIndex("dbo.YolcuUcakIstatistiks", new[] { "Border" });
            DropIndex("dbo.UserBureaux", "IX_UserBureau");
            DropIndex("dbo.UserAssignments", new[] { "Border" });
            DropIndex("dbo.UserAssignments", new[] { "Sicil" });
            DropIndex("dbo.TahditKayits", new[] { "Border" });
            DropIndex("dbo.InadYolcus", new[] { "Border" });
            DropIndex("dbo.HaftalikOlayCizelgesis", new[] { "Border" });
            DropIndex("dbo.GunlukZamanSerisiYolcus", new[] { "Border" });
            DropIndex("dbo.Bureaux", "IX_Bureau_Border_Code");
            DropColumn("dbo.YolcuUcakIstatistiks", "Border");
            DropColumn("dbo.TahditKayits", "Border");
            DropColumn("dbo.InadYolcus", "Border");
            DropColumn("dbo.HaftalikOlayCizelgesis", "Border");
            DropColumn("dbo.GunlukZamanSerisiYolcus", "Border");
            DropTable("dbo.UserBureaux");
            DropTable("dbo.UserAssignments");
            DropTable("dbo.Bureaux");
        }
    }
}
