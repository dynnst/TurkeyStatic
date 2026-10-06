const fs = require('fs');
const path = require('path');

const bad = "\uFFFD";

const replacements = {
    [`Veri Giri${bad}i`]: "Veri Girişi",
    [`Veri Giri${bad}`]: "Veri Girişi",
    [`B${bad}ro Se${bad}imi`]: "Büro Seçimi",
    [`Havaliman${bad} bilginiz`]: "Havalimanı bilginiz",
    [`Havaliman${bad}nda`]: "Havalimanında",
    [`tan${bad}ml${bad} de${bad}il`]: "tanımlı değil",
    [`y${bad}zden b${bad}ro`]: "yüzden büro",
    [`atanm${bad}al${bad}d${bad}r`]: "atanmalıdır",
    [`atanm${bad} bir`]: "atanmış bir",
    [`Y${bad}netimi ekran${bad}ndan`]: "Yönetimi ekranından",
    [`atanmal${bad}d${bad}r`]: "atanmalıdır",
    [`girece${bad}iniz b${bad}royu se${bad}in`]: "gireceğiniz büroyu seçin",
    [`B${bad}ro`]: "Büro",
    [`b${bad}ro`]: "büro",
    [`atanm${bad}`]: "atanmış",
    [`Su${bad} ${bad}nleme`]: "Suç Önleme",
    [`G${bad}nl${bad}k`]: "Günlük",
    [`Pasaport B${bad}rosu`]: "Pasaport Bürosu",
    [`T${bad}r${bad}`]: "Türü",
    [`Kay${bad}t`]: "Kayıt",
    [`${bad}leti${bad}im`]: "İletişim",
    [`${bad}zellikler`]: "Özellikler",
    [`${bad}nAd`]: "İnAd",
    [`${bad}nad`]: "İnad",
    [`A${bad}k`]: "Açık",
    [`${bad}zel`]: "Özel",
    [`G${bad}revli`]: "Görevli",
    [`${bad}eklinde`]: "Şeklinde",
    [`${bad}artlar`]: "Şartlar",
    [`${bad}lke`]: "Ülke",
    [`U${bad}ak`]: "Uçak",
    [`Haftal${bad}k`]: "Haftalık",
    [`${bad}likin`]: "İlişkin",
    [`De${bad}itir`]: "Değiştir",
    [`K${bad}mlatif`]: "Kümülatif",
    [`${bad}arma`]: "Çağırma",
    [`D${bad}`]: "Dış",
    [`${bad}`]: "İç", // Wait, replacing a single \uFFFD is dangerous, it will replace everything! Let's NOT replace a single \uFFFD!
};

// Remove single \uFFFD from map to prevent wildcard destruction
delete replacements[`${bad}`];

const files = [
    "C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Bureau/Index.cshtml",
    "C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Home/DataEntry.cshtml",
    "C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml"
];

files.forEach(f => {
    if (!fs.existsSync(f)) return;
    let content = fs.readFileSync(f, 'utf8');
    for (const [k, v] of Object.entries(replacements)) {
        content = content.split(k).join(v); // replaceAll
    }
    
    // Add specific regex replacements for some common broken words
    content = content.replace(new RegExp(`Su${bad}`, 'g'), "Suç");
    content = content.replace(new RegExp(`${bad}nleme`, 'g'), "Önleme");
    content = content.replace(new RegExp(`G${bad}nl${bad}k`, 'g'), "Günlük");
    content = content.replace(new RegExp(`B${bad}ro`, 'g'), "Büro");
    content = content.replace(new RegExp(`b${bad}ro`, 'g'), "büro");
    content = content.replace(new RegExp(`Se${bad}imi`, 'g'), "Seçimi");
    content = content.replace(new RegExp(`Havaliman${bad}`, 'g'), "Havalimanı");
    content = content.replace(new RegExp(`tan${bad}ml${bad}`, 'g'), "tanımlı");
    content = content.replace(new RegExp(`de${bad}il`, 'g'), "değil");
    content = content.replace(new RegExp(`y${bad}zden`, 'g'), "yüzden");
    content = content.replace(new RegExp(`atanm${bad}`, 'g'), "atanmış");
    content = content.replace(new RegExp(`Y${bad}netimi`, 'g'), "Yönetimi");
    content = content.replace(new RegExp(`ekran${bad}ndan`, 'g'), "ekranından");
    content = content.replace(new RegExp(`atanmal${bad}d${bad}r`, 'g'), "atanmalıdır");
    content = content.replace(new RegExp(`girece${bad}iniz`, 'g'), "gireceğiniz");
    content = content.replace(new RegExp(`se${bad}in`, 'g'), "seçin");
    content = content.replace(new RegExp(`Sular`, 'g'), "Suçlar");
    content = content.replace(new RegExp(`${bad}pheliler`, 'g'), "Şüpheliler");
    content = content.replace(new RegExp(`${bad}lemler`, 'g'), "İşlemler");
    content = content.replace(new RegExp(`${bad}cra`, 'g'), "İcra");
    content = content.replace(new RegExp(`Kaynakl${bad}`, 'g'), "Kaynaklı");
    content = content.replace(new RegExp(`Say${bad}s${bad}`, 'g'), "Sayısı");
    content = content.replace(new RegExp(`${bad}ehir`, 'g'), "Şehir");
    content = content.replace(new RegExp(`${bad}rn:`, 'g'), "Örn:");
    content = content.replace(new RegExp(`Y${bad}kleniyor...`, 'g'), "Yükleniyor...");
    content = content.replace(new RegExp(`g${bad}ncellendi`, 'g'), "güncellendi");
    content = content.replace(new RegExp(`ba${bad}ar${bad}yla`, 'g'), "başarıyla");
    content = content.replace(new RegExp(`olu${bad}turuldu`, 'g'), "oluşturuldu");
    content = content.replace(new RegExp(`olu${bad}tu`, 'g'), "oluştu");
    content = content.replace(new RegExp(`se${bad}ilmedi`, 'g'), "seçilmedi");
    content = content.replace(new RegExp(`ge${bad}ersiz`, 'g'), "geçersiz");
    content = content.replace(new RegExp(`d${bad}zenleyin`, 'g'), "düzenleyin");
    content = content.replace(new RegExp(`Biti${bad}`, 'g'), "Bitiş");
    content = content.replace(new RegExp(`Ba${bad}lang${bad}`, 'g'), "Başlangıç");
    content = content.replace(new RegExp(`gerek${bad}esi`, 'g'), "gerekçesi");
    content = content.replace(new RegExp(`Gidi${bad}`, 'g'), "Gidiş");
    content = content.replace(new RegExp(`Geli${bad}`, 'g'), "Geliş");
    content = content.replace(new RegExp(`T${bad}r${bad}`, 'g'), "Türü");
    content = content.replace(new RegExp(`A${bad}klama`, 'g'), "Açıklama");
    content = content.replace(new RegExp(`D${bad}${bad}`, 'g'), "Dış");
    content = content.replace(new RegExp(`Y${bad}n`, 'g'), "Yön");
    content = content.replace(new RegExp(`G${bad}r${bad}nt${bad}le`, 'g'), "Görüntüle");
    content = content.replace(new RegExp(`Kapal${bad}`, 'g'), "Kapalı");
    content = content.replace(new RegExp(`Giri${bad}i`, 'g'), "Girişi");
    content = content.replace(new RegExp(`Giri${bad}`, 'g'), "Girişi");

    fs.writeFileSync(f, "\uFEFF" + content, 'utf8'); // ensure BOM
});
