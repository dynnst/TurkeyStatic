namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPassportTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GunlukZamanSerisiYolcus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tarih = c.DateTime(nullable: false),
                        Yil = c.Int(nullable: false),
                        Ay = c.Int(nullable: false),
                        Gun = c.Int(nullable: false),
                        Yon = c.Int(nullable: false),
                        HatTuru = c.Int(nullable: false),
                        GunlukYolcuSayisi = c.Int(nullable: false),
                        GunlukKumulatifToplam = c.Int(nullable: false),
                        OnAylikToplam = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HaftalikOlayCizelgesis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TarihAraligi = c.String(maxLength: 100),
                        Havalimani = c.String(maxLength: 200),
                        SorgulananSahisSayisi = c.Int(nullable: false),
                        ArananSahisSayisi = c.Int(nullable: false),
                        SahteBelgeSayisi = c.Int(nullable: false),
                        InadEdilenSayisi = c.Int(nullable: false),
                        YazilanCezaMiktari = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrafiktenMenSayisi = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InadYolcus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiraNo = c.Int(nullable: false),
                        Tarih = c.DateTime(nullable: false),
                        AdSoyad = c.String(maxLength: 200),
                        Uyruk = c.String(maxLength: 100),
                        DogumTarihi = c.DateTime(),
                        PasaportNo = c.String(maxLength: 50),
                        GeldigiUlke = c.String(maxLength: 100),
                        GittigiUlke = c.String(maxLength: 100),
                        HavayoluSirketi = c.String(maxLength: 200),
                        InadGerekcesi = c.String(maxLength: 500),
                        Aciklamalar = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TahditKayits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tarih = c.DateTime(nullable: false),
                        AdSoyad = c.String(maxLength: 200),
                        Uyruk = c.String(maxLength: 100),
                        DogumTarihi = c.DateTime(),
                        PasaportVeyaKimlikNo = c.String(maxLength: 100),
                        TahditKodu = c.String(maxLength: 20),
                        Neden = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.YolcuUcakIstatistiks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Yil = c.Int(nullable: false),
                        Ay = c.Int(nullable: false),
                        HatTuru = c.Int(nullable: false),
                        GelenYolcu = c.Int(nullable: false),
                        GidenYolcu = c.Int(nullable: false),
                        ToplamYolcu = c.Int(nullable: false),
                        GelenUcak = c.Int(nullable: false),
                        GidenUcak = c.Int(nullable: false),
                        ToplamUcak = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.YolcuUcakIstatistiks");
            DropTable("dbo.TahditKayits");
            DropTable("dbo.InadYolcus");
            DropTable("dbo.HaftalikOlayCizelgesis");
            DropTable("dbo.GunlukZamanSerisiYolcus");
        }
    }
}
