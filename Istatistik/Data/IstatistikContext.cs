using System.Data.Entity;

namespace Istatistik.Models
{
    public class IstatistikContext : DbContext
    {
        static IstatistikContext()
        {
            // Şema Code First Migrations ile yönetilir; CreateDatabaseIfNotExists
            // model özetini __MigrationHistory ile karşılaştırıp aksi halde çökertmesin.
            Database.SetInitializer<IstatistikContext>(null);
        }

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
        public DbSet<YolcuUcakIstatistik> YolcuUcakIstatistikleri { get; set; }
        public DbSet<InadYolcu> InadYolcular { get; set; }
        public DbSet<TahditKayit> TahditKayitlari { get; set; }
        public DbSet<GunlukZamanSerisiYolcu> GunlukZamanSerisiYolcular { get; set; }
        public DbSet<HaftalikOlayCizelgesi> HaftalikOlayCizelgeleri { get; set; }
        public DbSet<Bureau> Bureaus { get; set; }
        public DbSet<UserAssignment> UserAssignments { get; set; }
        public DbSet<UserBureau> UserBureaus { get; set; }

        public DbSet<IdariBuroIstatistik> IdariBuroIstatistikleri { get; set; }
        public DbSet<GuvenlikHizmetleriIstatistik> GuvenlikHizmetleriIstatistikleri { get; set; }
        public DbSet<BilgiTeknolojileriIstatistik> BilgiTeknolojileriIstatistikleri { get; set; }
        public DbSet<CctvIstatistik> CctvIstatistikleri { get; set; }
        public DbSet<TrafikIstatistik> TrafikIstatistikleri { get; set; }
        public DbSet<GbtUyapSorgu> GbtUyapSorgulari { get; set; }
        public DbSet<SucOnlemeIcmal> SucOnlemeIcmallari { get; set; }
        public DbSet<YtsSorgu> YtsSorgulari { get; set; }
        public DbSet<SeyahatBelgesiRiskAnaliz> SeyahatBelgesiRiskAnalizleri { get; set; }
    }
}