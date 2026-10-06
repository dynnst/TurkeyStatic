const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');

c = c.replace(
    /({\s*n:\s*'YurdaGirisCikisBelgeTalebi'[^}]+},)/,
    "$1\n                          { n: 'TahditEkleme', l: 'Tahdit Ekleme', t: 'int', w: 4 },\n                          { n: 'TahditKaldirma', l: 'Tahdit Kaldırma', t: 'int', w: 4 },"
);

c = c.replace(
    /(\[\s*'[^']+',\s*'YurdaGirisCikisBelgeTalebi'\s*\],)/,
    "$1\n                          ['Tahdit Ekleme', 'TahditEkleme'],\n                          ['Tahdit Kaldırma', 'TahditKaldirma'],"
);

fs.writeFileSync(p, c, 'utf8');
console.log('Done');
