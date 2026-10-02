using System.Data.Entity;

namespace Istatistik.Models
{
    public class IstatistikContext : DbContext
    {
        // ConnectionString adını varsayılan olarak projenin adı olan IstatistikContext alacaktır.
        // Web.config dosyanızdaki connectionString adı ile eşleşmesi gerekir.
        public IstatistikContext() : base("name=IstatistikContext")
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CrimePreventionActivity> CrimePreventionActivities { get; set; }
        public DbSet<CrimeStatistic> CrimeStatistics { get; set; }
        public DbSet<QueryStatistic> QueryStatistics { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<User> Users { get; set; }
    }
}