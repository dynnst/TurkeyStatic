const fs = require('fs');
const file = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml';
let code = fs.readFileSync(file, 'utf8');

// 1. Add COUNTRIES and INAD_REASONS
const scriptVars = `        var YON = { 0: 'Gelen', 1: 'Giden' };

        var COUNTRIES = ["ABD", "Afganistan", "Almanya", "Andorra", "Angola", "Antigua ve Barbuda", "Arjantin", "Arnavutluk", "Avustralya", "Avusturya", "Azerbaycan", "Bahamalar", "Bahreyn", "Bangladeş", "Barbados", "Belçika", "Belize", "Benin", "Beyaz Rusya", "Bhutan", "Birleşik Arap Emirlikleri", "Birleşik Krallık", "Bolivya", "Bosna Hersek", "Botsvana", "Brezilya", "Brunei", "Bulgaristan", "Burkina Faso", "Burundi", "Cezayir", "Cibuti", "Çad", "Çekya", "Çin", "Danimarka", "Dominik Cumhuriyeti", "Dominika", "Ekvador", "Ekvator Ginesi", "El Salvador", "Endonezya", "Eritre", "Ermenistan", "Estonya", "Esvatini", "Etiyopya", "Fas", "Fiji", "Fildişi Sahili", "Filipinler", "Filistin", "Finlandiya", "Fransa", "Gabon", "Gambiya", "Gana", "Gine", "Gine-Bissau", "Grenada", "Guatemala", "Guyana", "Güney Afrika", "Güney Kore", "Güney Sudan", "Gürcistan", "Haiti", "Hırvatistan", "Hindistan", "Hollanda", "Honduras", "Irak", "İran", "İrlanda", "İspanya", "İsrail", "İsveç", "İsviçre", "İtalya", "İzlanda", "Jamaika", "Japonya", "Kamboçya", "Kamerun", "Kanada", "Karadağ", "Katar", "Kazakistan", "Kenya", "Kıbrıs", "Kırgızistan", "Kiribati", "Kolombiya", "Komorlar", "Kongo", "Kosta Rika", "Kuveyt", "Kuzey Kore", "Kuzey Makedonya", "Küba", "Laos", "Lesotho", "Letonya", "Liberya", "Libya", "Liechtenstein", "Litvanya", "Lübnan", "Lüksemburg", "Macaristan", "Madagaskar", "Malavi", "Maldivler", "Malezya", "Mali", "Malta", "Marshall Adaları", "Mauritius", "Meksika", "Mısır", "Mikronezya", "Moğolistan", "Moldova", "Monako", "Moritanya", "Mozambik", "Myanmar", "Namibya", "Nauru", "Nepal", "Nikaragua", "Nijer", "Nijerya", "Norveç", "Orta Afrika Cumhuriyeti", "Özbekistan", "Pakistan", "Palau", "Panama", "Papua Yeni Gine", "Paraguay", "Peru", "Polonya", "Portekiz", "Romanya", "Ruanda", "Rusya", "Saint Kitts ve Nevis", "Saint Lucia", "Saint Vincent ve Grenadinler", "Samoa", "San Marino", "Sao Tome ve Principe", "Senegal", "Seyşeller", "Sırbistan", "Sierra Leone", "Singapur", "Slovakya", "Slovenya", "Solomon Adaları", "Somali", "Sri Lanka", "Sudan", "Surinam", "Suriye", "Suudi Arabistan", "Şili", "Tacikistan", "Tanzanya", "Tayland", "Togo", "Tonga", "Trinidad ve Tobago", "Tunus", "Tuvalu", "Türkiye", "Türkmenistan", "Uganda", "Ukrayna", "Umman", "Uruguay", "Ürdün", "Vanuatu", "Vatikan", "Venezuela", "Vietnam", "Yemen", "Yeni Zelanda", "Yeşil Burun Adaları", "Yunanistan", "Zambiya", "Zimbabve"];
        var INAD_REASONS = ["Vize İhlali", "Geçerli Seyahat Belgesi Olmaması", "Sahte Belge Kullanımı", "Kamu Düzeni/Güvenliği Tehdidi", "Yeterli Maddi Kaynak Gösterememe", "Geliş Amacını İzah Edememe", "Giriş Yasağı Bulunması", "Diğer Sebepler"];
`;
code = code.split("        var YON = { 0: 'Gelen', 1: 'Giden' };").join(scriptVars);

// 2. Remove SiraNo and update fields
code = code.split("{ n: 'SiraNo', l: 'Sıra No', t: 'int', w: 3, ph: 'Örn: 1', section: '<i class=\"bi bi-card-heading me-2\"></i>Kayıt ve Kimlik Bilgileri' },").join("");
code = code.split("{ n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4 },").join("{ n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 4, section: '<i class=\"bi bi-card-heading me-2\"></i>Kayıt ve Kimlik Bilgileri' },");
code = code.split("{ n: 'Uyruk', l: 'Uyruk', t: 'text', max: 100, w: 5, ph: 'Örn: IRN, IRQ, DEU' },").join("{ n: 'Uyruk', l: 'Uyruk', t: 'select_str', opts: COUNTRIES, w: 5 },");
code = code.split("{ n: 'HavayoluSirketi', l: 'Havayolu Şirketi', t: 'text', max: 200, w: 6, ph: 'Örn: THY, Pegasus' },").join("{ n: 'HavayoluSirketi', l: 'Havayolu Şirketi', t: 'text', list: 'havayoluList', max: 200, w: 6, ph: 'Örn: THY, Pegasus' },");
code = code.split("{ n: 'GeldigiUlke', l: 'Geldiği Ülke', t: 'text', max: 100, w: 4, ph: 'Ülke adı', section: '<i class=\"bi bi-geo-alt me-2\"></i>Güzergah ve Gerekçe' },").join("{ n: 'GeldigiUlke', l: 'Geldiği Ülke', t: 'select_str', opts: COUNTRIES, w: 4, section: '<i class=\"bi bi-geo-alt me-2\"></i>Güzergah ve Gerekçe' },");
code = code.split("{ n: 'GittigiUlke', l: 'Gittiği Ülke', t: 'text', max: 100, w: 4, ph: 'Gönderildiği Ülke' },").join("{ n: 'GittigiUlke', l: 'Gittiği Ülke', t: 'select_str', opts: COUNTRIES, w: 4 },");
// InadGerekcesi has lowercase ü in the actual file! The ph says 'Gönderildiği ülke'. Wait, I'll use regex for GittigiUlke to be safe.
code = code.replace(/{ n: 'GittigiUlke'[^\}]+},/, "{ n: 'GittigiUlke', l: 'Gittiği Ülke', t: 'select_str', opts: COUNTRIES, w: 4 },");
code = code.replace(/{ n: 'InadGerekcesi'[^\}]+},/, "{ n: 'InadGerekcesi', l: 'İnad Gerekçesi', t: 'select_str', opts: INAD_REASONS, w: 4 },");

// 3. Update buildForm elements
const buildFormSelectStr = `                if (f.t === 'select' || f.t === 'select_str') {
                    el = document.createElement('select');
                    el.className = 'form-select';
                    var optEmpty = document.createElement('option');
                    optEmpty.value = '';
                    optEmpty.textContent = 'Seçiniz...';
                    el.appendChild(optEmpty);
                    if (Array.isArray(f.opts)) {
                        f.opts.forEach(function (val) {
                            var o = document.createElement('option');
                            o.value = val;
                            o.textContent = val;
                            el.appendChild(o);
                        });
                    } else {
                        Object.keys(f.opts).forEach(function (k) {
                            var o = document.createElement('option');
                            o.value = k;
                            o.textContent = f.opts[k];
                            el.appendChild(o);
                        });
                    }
                } else if (f.t === 'textarea') {`;

code = code.replace(/                if \(f\.t === 'select'\) \{[\s\S]*?\} else if \(f\.t === 'textarea'\) \{/, buildFormSelectStr);

// 4. list attribute for input
const inputStr = `                } else {
                    el = document.createElement('input');
                    el.className = 'form-control';
                    el.type = (f.t === 'int' || f.t === 'dec') ? 'number' : (f.t === 'date' ? 'date' : 'text');
                    if (f.t === 'int') { el.min = '0'; el.step = '1'; }
                    if (f.t === 'dec') { el.min = '0'; el.step = '0.01'; }
                    el.placeholder = f.ph || f.l;
                    if (f.list) el.setAttribute('list', f.list);
                }`;
code = code.replace(/                \} else \{[\s\S]*?el\.placeholder = f\.ph \|\| f\.l;\s*\}/, inputStr);

// 5. datalist append before modal show
const datalistGenStr = `            var datalistsWrap = document.getElementById('datalistsWrap');
            if (!datalistsWrap) {
                datalistsWrap = document.createElement('div');
                datalistsWrap.id = 'datalistsWrap';
                wrap.appendChild(datalistsWrap);
            }
            datalistsWrap.innerHTML = '';
            
            if (current === 'inad') {
                var airlines = Array.from(new Set(rows.map(function(r) { return r.HavayoluSirketi; }).filter(Boolean)));
                var dl = document.createElement('datalist');
                dl.id = 'havayoluList';
                airlines.forEach(function(a) {
                    var opt = document.createElement('option');
                    opt.value = a;
                    dl.appendChild(opt);
                });
                datalistsWrap.appendChild(dl);
            }
            
            bootstrap.Modal.getOrCreateInstance`;

code = code.replace(/            bootstrap\.Modal\.getOrCreateInstance/, datalistGenStr);

// 6. collect() update
const collectStr = `                if (f.t === 'int') {
                    obj[f.n] = raw === '' ? 0 : parseInt(raw, 10);
                } else if (f.t === 'dec') {
                    obj[f.n] = raw === '' ? 0 : parseFloat(raw);
                } else if (f.t === 'select') {
                    obj[f.n] = raw === '' ? null : parseInt(raw, 10);
                } else if (f.t === 'select_str') {
                    obj[f.n] = raw ? raw.trim() : '';
                } else {`;
// Find the exact block in collect() function
const collectFind = `                if (f.t === 'int') {
                    obj[f.n] = raw === '' ? 0 : parseInt(raw, 10);
                } else if (f.t === 'dec') {
                    obj[f.n] = raw === '' ? 0 : parseFloat(raw);
                } else if (f.t === 'select') {
                    obj[f.n] = raw === '' ? null : parseInt(raw, 10);
                } else {`;
code = code.replace(collectFind, collectStr);

fs.writeFileSync(file, code, 'utf8');
