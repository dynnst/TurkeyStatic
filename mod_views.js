const fs = require('fs');
let dataHtml = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Data/Index.cshtml', 'utf8');

// Update CCTV fields
dataHtml = dataHtml.replace(/SabitSayisi/g, 'IpSabit');
dataHtml = dataHtml.replace(/HareketliSayisi/g, 'AnalogSabit'); // temporary just to clean up string

// Completely replace CCTV config
dataHtml = dataHtml.replace(/cctv: \{[\s\S]*?\}\s*\},/, 
`cctv: {
                    title: 'CCTV Kamera İstatistiği', search: false, year: true, icon: 'bi-camera-video',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Kamera Sayıları' },
                        { n: 'Bolge', l: 'Bölge', t: 'select_str', opts: ['Terminal Dışı', 'Terminal İçi', 'Pist-Apron', 'Apron'], req: true, w: 4 },
                        { n: 'IpSabit', l: 'IP Sabit Kamera', t: 'int', w: 3, section: 'Kamera Türleri' },
                        { n: 'IpHareketli', l: 'IP Hareketli Kamera', t: 'int', w: 3 },
                        { n: 'AnalogSabit', l: 'Analog Sabit Kamera', t: 'int', w: 3 },
                        { n: 'AnalogHareketli', l: 'Analog Hareketli', t: 'int', w: 3 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Bölge', 'Bolge'],
                        ['IP Sabit', 'IpSabit'],
                        ['IP Hareketli', 'IpHareketli'],
                        ['Analog Sabit', 'AnalogSabit'],
                        ['Analog Hareketli', 'AnalogHareketli']
                    ]
                }
            },`);

// Replace BILGI_TEK fields
dataHtml = dataHtml.replace(/bilgitek: \{[\s\S]*?PtsPlakaKayip'\s*\}\s*\]/, 
`bilgitek: {
                    title: 'Bilişim ve Teknoloji (Günlük)', search: false, year: true, icon: 'bi-pc-display',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Günlük İstatistikler' },
                        { n: 'KameraKaydiIncelemesi', l: 'Kamera Kaydı İncelemesi', t: 'int', w: 4 },
                        { n: 'TahditBakilanSorunluYolcu', l: 'Tahdit Bakılan Sorunlu Yolcu', t: 'int', w: 4 },
                        { n: 'YurdaGirisCikisBelgeTalebi', l: 'Giriş Çıkış Belge Talebi', t: 'int', w: 4 },
                        { n: 'TahditEkleme', l: 'Tahdit Ekleme', t: 'int', w: 4 },
                        { n: 'TahditKaldirma', l: 'Tahdit Kaldırma', t: 'int', w: 4 },
                        { n: 'PtsAracAraniyor', l: 'PTS: Araç Aranıyor', t: 'int', w: 3, section: 'PTS Verileri' },
                        { n: 'PtsAracCalinti', l: 'PTS: Araç Çalıntı', t: 'int', w: 3 },
                        { n: 'PtsPlakaCalinti', l: 'PTS: Plaka Çalıntı', t: 'int', w: 3 },
                        { n: 'PtsPlakaKayip', l: 'PTS: Plaka Kayıp', t: 'int', w: 3 }
                    ]`);

// Update SUC_ONLEME 
dataHtml = dataHtml.replace(/var SUC_TURLERI = \[[\s\S]*?\];/, 
`var SUC_TURLERI = [
                "Resmi Belgede Sahtecilik", 
                "Kişilerin Huzur ve Sükununu Bozma",
                "Güveni Kötüye Kullanmak (Müracaat)",
                "Mala Zarar Verme",
                "İş Kazası",
                "Açıktan Hırsızlık",
                "İşyerinden ve Kurumdan Hırsızlık",
                "Kayıp Eşya",
                "Görevi Yaptırmamak İçin Direnme-Memura Mukavemet",
                "Nitelikli Dolandırıcılık",
                "Hakaret",
                "6136 SKM",
                "Trafik Güvenliğini Tehlikeye Sokmak",
                "Kasten Yaralama",
                "Taksirle Yaralama",
                "GBT-UYAP API-PNR GÖZCÜ PRJ. ARANAN KİŞİ",
                "İdari Para Cezası Uygulananlar",
                "Diğer (Açıklamaya Yazınız)"
            ];`);

// Add Idari and Guvenlik blocks
dataHtml = dataHtml.replace(/if \(BUREAU === 'BILGI_TEK'\) \{/, 
`if (BUREAU === 'IDARI') {
            TYPES = {
                idari: {
                    title: 'İdari Büro', search: false, year: true, icon: 'bi-file-earmark-text',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Evrak ve Personel' },
                        { n: 'GelenEvrak', l: 'Gelen Evrak', t: 'int', w: 4 },
                        { n: 'GidenEvrak', l: 'Giden Evrak', t: 'int', w: 4 },
                        { n: 'ToplamPersonelSayisi', l: 'Toplam Personel', t: 'int', w: 4 },
                        { n: 'IdariBuroPersonelSayisi', l: 'İdari Büro Personeli', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Gelen Evrak', 'GelenEvrak'],
                        ['Giden Evrak', 'GidenEvrak'],
                        ['Toplam Per.', 'ToplamPersonelSayisi'],
                        ['İdari Per.', 'IdariBuroPersonelSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'GUVENLIK') {
            TYPES = {
                guvenlik: {
                    title: 'Güvenlik Hizmetleri (Ana Nizamiye)', search: false, year: true, icon: 'bi-shield-lock',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Araç İstatistikleri' },
                        { n: 'GirisYapanArac', l: 'Giriş Yapan Araç', t: 'int', w: 4 },
                        { n: 'KontrolEdilenArac', l: 'Kontrol Edilen Araç', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Giriş Yapan', 'GirisYapanArac'],
                        ['Kontrol Edilen', 'KontrolEdilenArac']
                    ]
                }
            };
        } else if (BUREAU === 'BILGI_TEK') {`);

fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Data/Index.cshtml', dataHtml, 'utf8');

// Update INAD_REASONS in Passport/Index.cshtml
let passportHtml = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml', 'utf8');
passportHtml = passportHtml.replace(/var INAD_REASONS = \[[\s\S]*?\];/, 
`var INAD_REASONS = [
            "Geçerli Seyahat Belgesine Sahip Olmayan",
            "Türkiye'ye Girişi Sakıncalı Görülmesi",
            "Yeterli Maddi İmkana Sahip Olmaması",
            "Hakkında Türkiye'ye Giriş Yasağı Bulunması",
            "Giriş Vizesinin Olmaması",
            "Pasaport Süresinin Yetersiz Olması",
            "Sahte Pasaport",
            "Pasaportta Tahrifat Yapılmış Olması",
            "Sahte Vize İkamet",
            "Çalıntı Belge",
            "180 Günde 90 Gün Uygulamasının İhlali",
            "Türkiye'ye Girmek İstememesi",
            "Diğer Sebepler"
        ];`);
fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml', passportHtml, 'utf8');

console.log("Updated Views.");
