const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');

c = c.replace(
    /var btnEdit = document.createElement\('button'\);\s*btnEdit.className = 'btn btn-sm btn-outline-primary me-1 rounded-pill px-2';/,
    "var btnEdit = document.createElement('button');\n                    btnEdit.setAttribute('data-bs-toggle', 'modal');\n                    btnEdit.setAttribute('data-bs-target', '#recordModal');\n                    btnEdit.className = 'btn btn-sm btn-outline-primary me-1 rounded-pill px-2';"
);

fs.writeFileSync(p, c, 'utf8');
console.log('Done');
