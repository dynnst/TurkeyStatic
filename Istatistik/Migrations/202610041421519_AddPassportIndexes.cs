namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPassportIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.GunlukZamanSerisiYolcus", "Tarih");
            CreateIndex("dbo.GunlukZamanSerisiYolcus", "HatTuru");
            CreateIndex("dbo.InadYolcus", "Tarih");
            CreateIndex("dbo.InadYolcus", "Uyruk");
            CreateIndex("dbo.InadYolcus", "PasaportNo");
            CreateIndex("dbo.TahditKayits", "Tarih");
            CreateIndex("dbo.TahditKayits", "Uyruk");
            CreateIndex("dbo.TahditKayits", "PasaportVeyaKimlikNo");
            CreateIndex("dbo.YolcuUcakIstatistiks", "HatTuru");
        }
        
        public override void Down()
        {
            DropIndex("dbo.YolcuUcakIstatistiks", new[] { "HatTuru" });
            DropIndex("dbo.TahditKayits", new[] { "PasaportVeyaKimlikNo" });
            DropIndex("dbo.TahditKayits", new[] { "Uyruk" });
            DropIndex("dbo.TahditKayits", new[] { "Tarih" });
            DropIndex("dbo.InadYolcus", new[] { "PasaportNo" });
            DropIndex("dbo.InadYolcus", new[] { "Uyruk" });
            DropIndex("dbo.InadYolcus", new[] { "Tarih" });
            DropIndex("dbo.GunlukZamanSerisiYolcus", new[] { "HatTuru" });
            DropIndex("dbo.GunlukZamanSerisiYolcus", new[] { "Tarih" });
        }
    }
}
