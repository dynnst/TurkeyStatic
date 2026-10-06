const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');
c = c.replace(/URLS = \{ list: '\/Data\/Get', save: '\/Data\/Save', del: '\/Data\/Delete' \}/g, "URLS = { list: '/BureauData/Get', save: '/BureauData/Save', del: '/BureauData/Delete' }");
fs.writeFileSync(p, c, 'utf8');
console.log("Done");
