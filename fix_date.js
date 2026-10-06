const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');

// Replace all inline date formatters with fmtDate
c = c.replace(/function\s*\(v\)\s*\{\s*return v \? v\.substring\(0,10\) : '-';\s*\}/g, 'fmtDate');

fs.writeFileSync(p, c, 'utf8');
console.log('Done. Replacements made.');
