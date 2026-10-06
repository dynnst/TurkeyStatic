const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');

c = c.replace(
    "var BUREAU = '@ViewBag.Bureau';",
    "var editingId = 0;\n        var rows = [];\n        var currentPage = 1;\n        var pageSize = 15;\n        var current;\n        var BUREAU = '@ViewBag.Bureau';"
);

fs.writeFileSync(p, c, 'utf8');
console.log('Done');
