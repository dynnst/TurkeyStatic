const fs = require('fs');
let p = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/BureauData/Index.cshtml';
let c = fs.readFileSync(p, 'utf8');

let tabsCode = `
        var tabContainer = $('ppTabs');
        if (tabContainer) {
            tabContainer.innerHTML = '';
            var isFirst = true;
            Object.keys(TYPES).forEach(function(k) {
                var cfg = TYPES[k];
                if (isFirst) { current = k; }
                var li = document.createElement('li');
                li.className = 'nav-item';
                var btn = document.createElement('button');
                btn.className = 'nav-link px-3 py-2 fw-medium border-0 ' + (isFirst ? 'active' : '');
                btn.setAttribute('data-type', k);
                btn.innerHTML = '<i class="bi ' + (cfg.icon || 'bi-table') + ' me-2"></i>' + cfg.title;
                li.appendChild(btn);
                tabContainer.appendChild(li);
                isFirst = false;
            });
        }
`;

c = c.replace(
    "resetForm();\n        load();",
    tabsCode + "\n        resetForm();\n        load();"
);

fs.writeFileSync(p, c, 'utf8');
console.log('Done');
