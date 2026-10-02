namespace Istatistik.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        AuditLogId = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(),
                        Username = c.String(nullable: false, maxLength: 100),
                        Action = c.String(nullable: false, maxLength: 50),
                        EntityType = c.String(nullable: false, maxLength: 100),
                        EntityId = c.Int(nullable: false),
                        OldValue = c.String(maxLength: 1000),
                        NewValue = c.String(maxLength: 1000),
                        ActionDate = c.DateTime(nullable: false),
                        IPAddress = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.AuditLogId)
                .ForeignKey("dbo.Units", t => t.UnitId)
                .Index(t => t.UnitId);
            
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        UnitId = c.Int(nullable: false, identity: true),
                        UnitName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UnitId);
            
            CreateTable(
                "dbo.CrimePreventionActivities",
                c => new
                    {
                        ActivityId = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(nullable: false),
                        EntryDate = c.DateTime(nullable: false),
                        WarrantSource = c.String(maxLength: 50),
                        WarrantCount = c.Int(nullable: false),
                        ApprehendedCount = c.Int(nullable: false),
                        ApprehensionLocation = c.String(maxLength: 200),
                        ArrestedCount = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 100),
                        ModifiedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.ActivityId)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId);
            
            CreateTable(
                "dbo.CrimeStatistics",
                c => new
                    {
                        CrimeStatisticId = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(nullable: false),
                        EntryDate = c.DateTime(nullable: false),
                        CrimeType = c.String(nullable: false, maxLength: 100),
                        CrimeCode = c.String(maxLength: 20),
                        CasesRequiringFollow_up = c.Int(nullable: false),
                        SuspectCount = c.Int(nullable: false),
                        ArrestedCount = c.Int(nullable: false),
                        JudicialControlCount = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 100),
                        ModifiedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.CrimeStatisticId)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId);
            
            CreateTable(
                "dbo.QueryStatistics",
                c => new
                    {
                        QueryStatisticId = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(nullable: false),
                        EntryDate = c.DateTime(nullable: false),
                        Shift = c.String(nullable: false, maxLength: 50),
                        PersonQueriedCount = c.Int(nullable: false),
                        PersonArrestedSearchedCount = c.Int(nullable: false),
                        OperationType = c.String(maxLength: 50),
                        Notes = c.String(maxLength: 500),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 100),
                        ModifiedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.QueryStatisticId)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 256),
                        FullName = c.String(nullable: false, maxLength: 256),
                        PasswordHash = c.String(nullable: false, maxLength: 256),
                        UnitId = c.Int(nullable: false),
                        Role = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "UnitId", "dbo.Units");
            DropForeignKey("dbo.QueryStatistics", "UnitId", "dbo.Units");
            DropForeignKey("dbo.CrimeStatistics", "UnitId", "dbo.Units");
            DropForeignKey("dbo.CrimePreventionActivities", "UnitId", "dbo.Units");
            DropForeignKey("dbo.AuditLogs", "UnitId", "dbo.Units");
            DropIndex("dbo.Users", new[] { "UnitId" });
            DropIndex("dbo.QueryStatistics", new[] { "UnitId" });
            DropIndex("dbo.CrimeStatistics", new[] { "UnitId" });
            DropIndex("dbo.CrimePreventionActivities", new[] { "UnitId" });
            DropIndex("dbo.AuditLogs", new[] { "UnitId" });
            DropTable("dbo.Users");
            DropTable("dbo.QueryStatistics");
            DropTable("dbo.CrimeStatistics");
            DropTable("dbo.CrimePreventionActivities");
            DropTable("dbo.Units");
            DropTable("dbo.AuditLogs");
        }
    }
}
