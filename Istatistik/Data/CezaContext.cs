using System.Data.Entity;

namespace Istatistik.Models
{
    /// <summary>
    /// CEZA veritabanı. Şema yönetilmez; yalnızca mevcut E_User tablosu okunur.
    /// </summary>
    public class CezaContext : DbContext
    {
        static CezaContext()
        {
            Database.SetInitializer<CezaContext>(null);
        }

        public CezaContext() : base("name=CezaContext")
        {
        }

        public DbSet<EUser> EUsers { get; set; }
    }
}
