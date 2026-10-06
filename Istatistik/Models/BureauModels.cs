using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    public class IdariBuroIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }
        [Required] [StringLength(150)] [Index] public string Border { get; set; }
        [Index] public DateTime Tarih { get; set; }
        
        public int GelenEvrak { get; set; }
        public int GidenEvrak { get; set; }
        public int ToplamPersonelSayisi { get; set; }
        public int IdariBuroPersonelSayisi { get; set; }
    }

    public class GuvenlikHizmetleriIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }
        [Required] [StringLength(150)] [Index] public string Border { get; set; }
        [Index] public DateTime Tarih { get; set; }

        public int GirisYapanArac { get; set; }
        public int KontrolEdilenArac { get; set; }
    }

    public class BilgiTeknolojileriIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        public int KameraKaydiIncelemesi { get; set; }
        
        public int PtsAracAraniyor { get; set; }
        public int PtsAracCalinti { get; set; }
        public int PtsPlakaCalinti { get; set; }
        public int PtsPlakaKayip { get; set; }
        
        public int TahditBakilanSorunluYolcu { get; set; }
        public int YurdaGirisCikisBelgeTalebi { get; set; }
        public int TahditEkleme { get; set; }
        public int TahditKaldirma { get; set; }
    }

    public class CctvIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        [StringLength(150)]
        public string Bolge { get; set; } // Örn: Terminal İçi, Terminal Dışı

        public int IpSabit { get; set; }
        public int IpHareketli { get; set; }
        public int AnalogSabit { get; set; }
        public int AnalogHareketli { get; set; }
    }

    public class TrafikIstatistik : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        public int KontrolEdilenAracSayisi { get; set; }
        public int CezaYazilanSurucuSayisi { get; set; }
        public decimal CezaTutari { get; set; }
        public int TrafiktenMenEdilenAracSayisi { get; set; }
        public int GeciciGeriAlinanSurucuBelgesi { get; set; }
    }

    public class GbtUyapSorgu : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        public int SorgulananKisiSayisi { get; set; }
        public int YakalananKisiSayisi { get; set; }
    }

    public class SucOnlemeIcmal : IBorderEntity
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
        [Index]
        public string SucTuru { get; set; }

        public int VakaSayisi { get; set; }
    }

    public class YtsSorgu : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        public int GunlukSorguSayisi { get; set; }
    }
    
    public enum Cinsiyet
    {
        Erkek = 0,
        Kadin = 1
    }

    public class SeyahatBelgesiRiskAnaliz : IBorderEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Index]
        public DateTime Tarih { get; set; }

        public Yon Yon { get; set; }
        public Cinsiyet Cinsiyet { get; set; }

        public int IslemSayisi { get; set; }
    }
}
