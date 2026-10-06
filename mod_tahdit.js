const fs = require('fs');
const file = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml';
let code = fs.readFileSync(file, 'utf8');

// 1. Replace TahditKodu
code = code.replace(
    /\{ n: 'TahditKodu', l: 'Tahdit Kodu', t: 'text', req: true, max: 20, w: 4, ph: '[^']+' \},/,
    "{ n: 'TahditKodu', l: 'Tahdit Kodu', t: 'text', list: 'tahditKoduList', req: true, max: 20, w: 4, ph: 'Örn: V-84, C-101, G-87' },"
);

// 2. Replace Uyruk in tahdit
code = code.replace(
    /\{ n: 'Uyruk', l: 'Uyruk', t: 'text', max: 100, w: 4, ph: '[^']+' \},/,
    "{ n: 'Uyruk', l: 'Uyruk', t: 'select_str', opts: COUNTRIES, w: 4 },"
);

// 3. Add datalist for tahditKodu
const newDatalistGenStr = `            if (current === 'inad') {
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
            if (current === 'tahdit') {
                var tahditKodlari = Array.from(new Set(rows.map(function(r) { return r.TahditKodu; }).filter(Boolean)));
                var dl2 = document.createElement('datalist');
                dl2.id = 'tahditKoduList';
                tahditKodlari.forEach(function(k) {
                    var opt = document.createElement('option');
                    opt.value = k;
                    dl2.appendChild(opt);
                });
                datalistsWrap.appendChild(dl2);
            }`;

code = code.replace(
    /            if \(current === 'inad'\) \{[\s\S]*?datalistsWrap\.appendChild\(dl\);\s*\}/,
    newDatalistGenStr
);

fs.writeFileSync(file, code, 'utf8');
