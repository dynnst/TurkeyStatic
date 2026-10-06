
namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SunumGuncellemeleri : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GuvenlikHizmetleriIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        GirisYapanArac = c.Int(nullable: false),
                        KontrolEdilenArac = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.IdariBuroIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        GelenEvrak = c.Int(nullable: false),
                        GidenEvrak = c.Int(nullable: false),
                        ToplamPersonelSayisi = c.Int(nullable: false),
                        IdariBuroPersonelSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            AddColumn("dbo.BilgiTeknolojileriIstatistiks", "TahditEkleme", c => c.Int(nullable: false));
            AddColumn("dbo.BilgiTeknolojileriIstatistiks", "TahditKaldirma", c => c.Int(nullable: false));
            AddColumn("dbo.CctvIstatistiks", "IpSabit", c => c.Int(nullable: false));
            AddColumn("dbo.CctvIstatistiks", "IpHareketli", c => c.Int(nullable: false));
            AddColumn("dbo.CctvIstatistiks", "AnalogSabit", c => c.Int(nullable: false));
            AddColumn("dbo.CctvIstatistiks", "AnalogHareketli", c => c.Int(nullable: false));
            DropColumn("dbo.CctvIstatistiks", "SabitSayisi");
            DropColumn("dbo.CctvIstatistiks", "HareketliSayisi");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CctvIstatistiks", "HareketliSayisi", c => c.Int(nullable: false));
            AddColumn("dbo.CctvIstatistiks", "SabitSayisi", c => c.Int(nullable: false));
            DropIndex("dbo.IdariBuroIstatistiks", new[] { "Tarih" });
            DropIndex("dbo.IdariBuroIstatistiks", new[] { "Border" });
            DropIndex("dbo.GuvenlikHizmetleriIstatistiks", new[] { "Tarih" });
            DropIndex("dbo.GuvenlikHizmetleriIstatistiks", new[] { "Border" });
            DropColumn("dbo.CctvIstatistiks", "AnalogHareketli");
            DropColumn("dbo.CctvIstatistiks", "AnalogSabit");
            DropColumn("dbo.CctvIstatistiks", "IpHareketli");
            DropColumn("dbo.CctvIstatistiks", "IpSabit");
            DropColumn("dbo.BilgiTeknolojileriIstatistiks", "TahditKaldirma");
            DropColumn("dbo.BilgiTeknolojileriIstatistiks", "TahditEkleme");
            DropTable("dbo.IdariBuroIstatistiks");
            DropTable("dbo.GuvenlikHizmetleriIstatistiks");
        }
    }
}
