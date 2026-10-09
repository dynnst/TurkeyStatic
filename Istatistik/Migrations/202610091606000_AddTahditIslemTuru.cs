namespace Istatistik.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddTahditIslemTuru : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TahditKayits", "IslemTuru", c => c.Int(nullable: false));
            CreateIndex("dbo.TahditKayits", "IslemTuru");
        }

        public override void Down()
        {
            DropIndex("dbo.TahditKayits", new[] { "IslemTuru" });
            DropColumn("dbo.TahditKayits", "IslemTuru");
        }
    }
}
