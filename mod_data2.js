const fs = require('fs');
let html = fs.readFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Data/Index.cshtml', 'utf8');

html = html.replace('ViewBag.Title = "Pasaport Bürosu - Veri Girişi";', 'ViewBag.Title = ViewBag.Bureau + " - Veri Girişi";');
html = html.replace('ViewBag.Title = "Pasaport Brosu - Veri Girii";', 'ViewBag.Title = ViewBag.Bureau + " - Veri Girişi";');

html = html.replace('<h3 class="mb-0 fw-bold text-dark">Pasaport Bürosu Veri Girişi</h3>', '<h3 class="mb-0 fw-bold text-dark">@ViewBag.Bureau Veri Girişi</h3>');
html = html.replace('<h3 class="mb-0 fw-bold text-dark">Pasaport Brosu Veri Girii</h3>', '<h3 class="mb-0 fw-bold text-dark">@ViewBag.Bureau Veri Girişi</h3>');

// And remove the hardcoded tabs:
html = html.replace(/<ul class="nav nav-pills gap-2 bg-light p-2 rounded-3 border mb-0" id="ppTabs">[\s\S]*?<\/ul>/, 
`<ul class="nav nav-pills gap-2 bg-light p-2 rounded-3 border mb-0" id="ppTabs">
    <!-- Tabs will be generated dynamically -->
</ul>`);

// Generate tabs in JS:
html = html.replace(/var current = Object\.keys\(TYPES\)\[0\];/, `
        var current = Object.keys(TYPES)[0];
        
        var tabsUl = document.getElementById('ppTabs');
        tabsUl.innerHTML = '';
        Object.keys(TYPES).forEach(function(k, idx) {
            var cfg = TYPES[k];
            var li = document.createElement('li');
            li.className = 'nav-item';
            
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'nav-link px-3 py-2 fw-semibold ' + (idx === 0 ? 'active' : '');
            btn.setAttribute('data-type', k);
            btn.innerHTML = '<i class="bi ' + (cfg.icon || 'bi-table') + ' me-1"></i> ' + cfg.title;
            
            li.appendChild(btn);
            tabsUl.appendChild(li);
        });
`);

fs.writeFileSync('C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Data/Index.cshtml', html, 'utf8');
