const fs = require('fs');
const passportHtml = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml', 'utf8');

let dataHtml = passportHtml.replace(/ViewBag\.Title = "Pasaport Büro Amirliği";/g, 'ViewBag.Title = ViewBag.Bureau + " Büro Amirliği";');
dataHtml = dataHtml.replace(/<h2 class="mb-0 text-white"><i class="bi bi-passport me-2"><\/i> Pasaport Büro Amirliği<\/h2>/g, '<h2 class="mb-0 text-white"><i class="bi bi-building me-2"></i> @ViewBag.Bureau Büro Demirbaşları</h2>');

dataHtml = dataHtml.replace(/URLS = \{[^}]+\}/, `URLS = { list: '/Data/Get', save: '/Data/Save', del: '/Data/Delete' }`);

// Now replace the TYPES dictionary with a dynamic one based on Bureau
dataHtml = dataHtml.replace(/var TYPES = \{[\s\S]*?var COUNTRIES =/m, `
        var COUNTRIES =`);

dataHtml = dataHtml.replace(/var fmtNum =/m, `
        var BUREAU = '@ViewBag.Bureau';
        var TYPES = {};

        if (BUREAU === 'BILGI_TEK') {
            TYPES = {
                bilgitek: {
                    title: 'Bilişim ve Teknoloji (Günlük)', search: false, year: true, icon: 'bi-pc-display',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Günlük İstatistikler' },
                        { n: 'KameraKaydiIncelemesi', l: 'Kamera Kaydı İncelemesi', t: 'int', w: 4 },
                        { n: 'TahditBakilanSorunluYolcu', l: 'Tahdit Bakılan Sorunlu Yolcu', t: 'int', w: 4 },
                        { n: 'YurdaGirisCikisBelgeTalebi', l: 'Giriş Çıkış Belge Talebi', t: 'int', w: 4 },
                        { n: 'PtsAracAraniyor', l: 'PTS: Araç Aranıyor', t: 'int', w: 4, section: 'PTS Verileri' },
                        { n: 'PtsAracCalinti', l: 'PTS: Araç Çalıntı', t: 'int', w: 4 },
                        { n: 'PtsPlakaCalinti', l: 'PTS: Plaka Çalıntı', t: 'int', w: 4 },
                        { n: 'PtsPlakaKayip', l: 'PTS: Plaka Kayıp', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Kamera', 'KameraKaydiIncelemesi'],
                        ['Tahdit/Yolcu', 'TahditBakilanSorunluYolcu'],
                        ['Giriş/Çıkış Belge', 'YurdaGirisCikisBelgeTalebi'],
                        ['Araç Aranan', 'PtsAracAraniyor']
                    ]
                },
                cctv: {
                    title: 'CCTV Kamera İstatistiği', search: false, year: true, icon: 'bi-camera-video',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Kamera Sayıları' },
                        { n: 'Bolge', l: 'Bölge', t: 'text', req: true, max: 150, w: 4 },
                        { n: 'SabitSayisi', l: 'Sabit Kamera Sayısı', t: 'int', w: 4 },
                        { n: 'HareketliSayisi', l: 'Hareketli Kamera Sayısı', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Bölge', 'Bolge'],
                        ['Sabit', 'SabitSayisi'],
                        ['Hareketli', 'HareketliSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'TRAFIK') {
            TYPES = {
                trafik: {
                    title: 'Trafik İstatistikleri', search: false, year: true, icon: 'bi-cone-striped',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Günlük Trafik İstatistikleri' },
                        { n: 'KontrolEdilenAracSayisi', l: 'Kontrol Edilen Araç Sayısı', t: 'int', w: 4 },
                        { n: 'CezaYazilanSurucuSayisi', l: 'Ceza Yazılan Sürücü Sayısı', t: 'int', w: 4 },
                        { n: 'CezaTutari', l: 'Ceza Tutarı (₺)', t: 'dec', w: 4 },
                        { n: 'TrafiktenMenEdilenAracSayisi', l: 'Trafikten Men Edilen Araç', t: 'int', w: 4 },
                        { n: 'GeciciGeriAlinanSurucuBelgesi', l: 'Geçici Geri Alınan Sürücü Belgesi', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Kontrol Araç', 'KontrolEdilenAracSayisi'],
                        ['Ceza (Sürücü)', 'CezaYazilanSurucuSayisi'],
                        ['Tutar', 'CezaTutari'],
                        ['Men Edilen', 'TrafiktenMenEdilenAracSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'GBT_UYAP') {
            TYPES = {
                gbtuyap: {
                    title: 'GBT ve UYAP Sorgulama', search: false, year: true, icon: 'bi-search',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'GBT / UYAP Verileri' },
                        { n: 'SorgulananKisiSayisi', l: 'Sorgulaması Yapılan Kişi Sayısı', t: 'int', w: 4 },
                        { n: 'YakalananKisiSayisi', l: 'Aranması Olup Yakalanan Kişi Sayısı', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Sorgulanan', 'SorgulananKisiSayisi'],
                        ['Yakalanan', 'YakalananKisiSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'SUC_ONLEME') {
            var SUC_TURLERI = [
                "Resmi Belgede Sahtecilik", 
                "Kişilerin Huzur ve Sükununu Bozma",
                "Kasten Yaralama",
                "Hırsızlık",
                "Dolandırıcılık",
                "Tehdit",
                "Hakaret",
                "Görevi Yaptırmamak İçin Direnme",
                "Mala Zarar Verme",
                "Uyuşturucu Madde Kullanma",
                "Uyuşturucu Madde Ticareti",
                "Kaçakçılık (5607 SKM)",
                "Silahlı Terör Örgütüne Üye Olma",
                "Göçmen Kaçakçılığı",
                "Taksirle Yaralama",
                "Diğer (Açıklamaya Yazınız)"
            ];
            TYPES = {
                suconleme: {
                    title: 'Suç Önleme ve Soruşturma İcmal', search: true, year: true, icon: 'bi-shield-check',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Vaka Girişi' },
                        { n: 'SucTuru', l: 'Suç Türü', t: 'select_str', opts: SUC_TURLERI, req: true, w: 4 },
                        { n: 'VakaSayisi', l: 'Vaka Sayısı', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Suç Türü', 'SucTuru'],
                        ['Vaka Sayısı', 'VakaSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'YTS_SORGU') {
            TYPES = {
                ytssorgu: {
                    title: 'YTS Sorgu Sayıları', search: false, year: true, icon: 'bi-search-heart',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Günlük Sorgu' },
                        { n: 'GunlukSorguSayisi', l: 'Günlük Sorgu Sayısı', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Sorgu Sayısı', 'GunlukSorguSayisi']
                    ]
                }
            };
        } else if (BUREAU === 'SEYAHAT_BELGE_RISK') {
            TYPES = {
                seyahat: {
                    title: 'Sahtecilikten İncelenen Belge', search: false, year: true, icon: 'bi-file-earmark-break',
                    fields: [
                        { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: 'Belge İstatistikleri' },
                        { n: 'Yon', l: 'Yön', t: 'select', opts: {0: 'Gelen', 1: 'Giden'}, w: 4 },
                        { n: 'Cinsiyet', l: 'Cinsiyet', t: 'select', opts: {0: 'Erkek', 1: 'Kadın'}, w: 4 },
                        { n: 'IslemSayisi', l: 'İşlem Yapılan Sayı', t: 'int', w: 4 }
                    ],
                    cols: [
                        ['Tarih', 'Tarih', function(v) { return v ? v.substring(0,10) : '-'; }],
                        ['Yön', 'Yon', function(v) { return v === 0 ? 'Gelen' : (v===1 ? 'Giden' : '-'); }],
                        ['Cinsiyet', 'Cinsiyet', function(v) { return v === 0 ? 'Erkek' : (v===1 ? 'Kadın' : '-'); }],
                        ['İşlem Sayısı', 'IslemSayisi']
                    ]
                }
            };
        }

        var current = Object.keys(TYPES)[0];

        var fmtNum =`);

// Quick fix for extra URL variable
dataHtml = dataHtml.replace(/var qs = 'type=' \+ encodeURIComponent\(current\);/, `var qs = 'bureau=' + encodeURIComponent(BUREAU) + '&type=' + encodeURIComponent(current);`);
dataHtml = dataHtml.replace(/URLS\.save, \{/g, `URLS.save + '?bureau=' + encodeURIComponent(BUREAU), {`);
dataHtml = dataHtml.replace(/URLS\.del, \{/g, `URLS.del + '?bureau=' + encodeURIComponent(BUREAU), {`);


// Quick fix for extra bureau param in form serialization
dataHtml = dataHtml.replace(/fd\.append\('type', current\);/g, `fd.append('type', current);\n        fd.append('bureau', BUREAU);`);

fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Data/Index.cshtml', dataHtml, 'utf8');
console.log('Done');
