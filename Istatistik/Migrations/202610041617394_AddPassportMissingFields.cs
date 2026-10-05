namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPassportMissingFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GunlukZamanSerisiYolcus", "UcakSayisi", c => c.Int(nullable: false));
            AddColumn("dbo.HaftalikOlayCizelgesis", "BaslangicTarihi", c => c.DateTime());
            AddColumn("dbo.HaftalikOlayCizelgesis", "BitisTarihi", c => c.DateTime());
            AddColumn("dbo.InadYolcus", "GelisTarihi", c => c.DateTime());
            AddColumn("dbo.InadYolcus", "GidisTarihi", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InadYolcus", "GidisTarihi");
            DropColumn("dbo.InadYolcus", "GelisTarihi");
            DropColumn("dbo.HaftalikOlayCizelgesis", "BitisTarihi");
            DropColumn("dbo.HaftalikOlayCizelgesis", "BaslangicTarihi");
            DropColumn("dbo.GunlukZamanSerisiYolcus", "UcakSayisi");
        }
    }
}
