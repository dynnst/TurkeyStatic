namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YeniBirimler : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BilgiTeknolojileriIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        KameraKaydiIncelemesi = c.Int(nullable: false),
                        PtsAracAraniyor = c.Int(nullable: false),
                        PtsAracCalinti = c.Int(nullable: false),
                        PtsPlakaCalinti = c.Int(nullable: false),
                        PtsPlakaKayip = c.Int(nullable: false),
                        TahditBakilanSorunluYolcu = c.Int(nullable: false),
                        YurdaGirisCikisBelgeTalebi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.CctvIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        Bolge = c.String(maxLength: 150),
                        SabitSayisi = c.Int(nullable: false),
                        HareketliSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.GbtUyapSorgus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        SorgulananKisiSayisi = c.Int(nullable: false),
                        YakalananKisiSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.SeyahatBelgesiRiskAnalizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        Yon = c.Int(nullable: false),
                        Cinsiyet = c.Int(nullable: false),
                        IslemSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.SucOnlemeIcmals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        SucTuru = c.String(maxLength: 200),
                        VakaSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih)
                .Index(t => t.SucTuru);
            
            CreateTable(
                "dbo.TrafikIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        KontrolEdilenAracSayisi = c.Int(nullable: false),
                        CezaYazilanSurucuSayisi = c.Int(nullable: false),
                        CezaTutari = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrafiktenMenEdilenAracSayisi = c.Int(nullable: false),
                        GeciciGeriAlinanSurucuBelgesi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
            CreateTable(
                "dbo.YtsSorgus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Border = c.String(nullable: false, maxLength: 150),
                        Tarih = c.DateTime(nullable: false),
                        GunlukSorguSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Border)
                .Index(t => t.Tarih);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.YtsSorgus", new[] { "Tarih" });
            DropIndex("dbo.YtsSorgus", new[] { "Border" });
            DropIndex("dbo.TrafikIstatistiks", new[] { "Tarih" });
            DropIndex("dbo.TrafikIstatistiks", new[] { "Border" });
            DropIndex("dbo.SucOnlemeIcmals", new[] { "SucTuru" });
            DropIndex("dbo.SucOnlemeIcmals", new[] { "Tarih" });
            DropIndex("dbo.SucOnlemeIcmals", new[] { "Border" });
            DropIndex("dbo.SeyahatBelgesiRiskAnalizs", new[] { "Tarih" });
            DropIndex("dbo.SeyahatBelgesiRiskAnalizs", new[] { "Border" });
            DropIndex("dbo.GbtUyapSorgus", new[] { "Tarih" });
            DropIndex("dbo.GbtUyapSorgus", new[] { "Border" });
            DropIndex("dbo.CctvIstatistiks", new[] { "Tarih" });
            DropIndex("dbo.CctvIstatistiks", new[] { "Border" });
            DropIndex("dbo.BilgiTeknolojileriIstatistiks", new[] { "Tarih" });
            DropIndex("dbo.BilgiTeknolojileriIstatistiks", new[] { "Border" });
            DropTable("dbo.YtsSorgus");
            DropTable("dbo.TrafikIstatistiks");
            DropTable("dbo.SucOnlemeIcmals");
            DropTable("dbo.SeyahatBelgesiRiskAnalizs");
            DropTable("dbo.GbtUyapSorgus");
            DropTable("dbo.CctvIstatistiks");
            DropTable("dbo.BilgiTeknolojileriIstatistiks");
        }
    }
}
