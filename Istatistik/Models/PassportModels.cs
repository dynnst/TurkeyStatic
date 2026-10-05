using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    public enum HatTuru
    {
        Ic = 0,
        Dis = 1
    }

    public enum Yon
    {
        Gelen = 0,
        Giden = 1
    }

    public class YolcuUcakIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Range(2000, 2100)]
        public int Yil { get; set; }

        [Range(1, 12)]
        public int Ay { get; set; }

        [Index]
        public HatTuru HatTuru { get; set; }

        public int GelenYolcu { get; set; }
        public int GidenYolcu { get; set; }
        public int ToplamYolcu { get; set; }

        public int GelenUcak { get; set; }
        public int GidenUcak { get; set; }
        public int ToplamUcak { get; set; }
    }

    public class InadYolcu : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        public int SiraNo { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        [StringLength(200)]
        public string AdSoyad { get; set; }

        [StringLength(100)]
        [Index]
        public string Uyruk { get; set; }

        public DateTime? DogumTarihi { get; set; }

        public DateTime? GelisTarihi { get; set; }
        public DateTime? GidisTarihi { get; set; }

        [StringLength(50)]
        [Index]
        public string PasaportNo { get; set; }

        [StringLength(100)]
        public string GeldigiUlke { get; set; }

        [StringLength(100)]
        public string GittigiUlke { get; set; }

        [StringLength(200)]
        public string HavayoluSirketi { get; set; }

        [StringLength(500)]
        public string InadGerekcesi { get; set; }

        [StringLength(1000)]
        public string Aciklamalar { get; set; }
    }

    public class TahditKayit : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        [StringLength(200)]
        public string AdSoyad { get; set; }

        [StringLength(100)]
        [Index]
        public string Uyruk { get; set; }

        public DateTime? DogumTarihi { get; set; }

        [StringLength(100)]
        [Index]
        public string PasaportVeyaKimlikNo { get; set; }

        [StringLength(20)]
        public string TahditKodu { get; set; }

        [StringLength(500)]
        public string Neden { get; set; }
    }

    public class GunlukZamanSerisiYolcu : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }
        public int Yil { get; set; }
        public int Ay { get; set; }
        public int Gun { get; set; }

        public Yon Yon { get; set; }

        [Index]
        public HatTuru HatTuru { get; set; }

        public int GunlukYolcuSayisi { get; set; }
        public int UcakSayisi { get; set; }
        public int GunlukKumulatifToplam { get; set; }
        public int OnAylikToplam { get; set; }
    }

    public class HaftalikOlayCizelgesi : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [StringLength(100)]
        public string TarihAraligi { get; set; }

        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }

        [StringLength(200)]
        public string Havalimani { get; set; }

        public int SorgulananSahisSayisi { get; set; }
        public int ArananSahisSayisi { get; set; }
        public int SahteBelgeSayisi { get; set; }
        public int InadEdilenSayisi { get; set; }

        public decimal YazilanCezaMiktari { get; set; }
        public int TrafiktenMenSayisi { get; set; }
    }
}
