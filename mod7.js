const fs = require('fs');
const file = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml';
let code = fs.readFileSync(file, 'utf8');

// Normalize line endings to \n for easier replacement
code = code.replace(/\r\n/g, '\n');

// 1. buildForm select logic
const selectFind = `                if (f.t === 'select') {
                    el = document.createElement('select');
                    el.className = 'form-select';
                    Object.keys(f.opts).forEach(function (k) {
                        var o = document.createElement('option');
                        o.value = k;
                        o.textContent = f.opts[k];
                        el.appendChild(o);
                    });
                } else if (f.t === 'textarea') {`;

const selectRep = `                if (f.t === 'select' || f.t === 'select_str') {
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

code = code.replace(selectFind, selectRep);

// 2. add datalist list parsing
const inputFind = `                } else {
                    el = document.createElement('input');
                    el.className = 'form-control';
                    el.type = (f.t === 'int' || f.t === 'dec') ? 'number' : (f.t === 'date' ? 'date' : 'text');
                    if (f.t === 'int') { el.min = '0'; el.step = '1'; }
                    if (f.t === 'dec') { el.min = '0'; el.step = '0.01'; }
                    el.placeholder = f.ph || f.l;
                }`;

const inputRep = `                } else {
                    el = document.createElement('input');
                    el.className = 'form-control';
                    el.type = (f.t === 'int' || f.t === 'dec') ? 'number' : (f.t === 'date' ? 'date' : 'text');
                    if (f.t === 'int') { el.min = '0'; el.step = '1'; }
                    if (f.t === 'dec') { el.min = '0'; el.step = '0.01'; }
                    el.placeholder = f.ph || f.l;
                    if (f.list) el.setAttribute('list', f.list);
                }`;

code = code.replace(inputFind, inputRep);

// 3. update collect logic
const collectFind = `                if (f.t === 'int') {
                    obj[f.n] = raw === '' ? 0 : parseInt(raw, 10);
                } else if (f.t === 'dec') {
                    obj[f.n] = raw === '' ? 0 : parseFloat(raw);
                } else if (f.t === 'select') {
                    obj[f.n] = raw === '' ? null : parseInt(raw, 10);
                } else {`;

const collectRep = `                if (f.t === 'int') {
                    obj[f.n] = raw === '' ? 0 : parseInt(raw, 10);
                } else if (f.t === 'dec') {
                    obj[f.n] = raw === '' ? 0 : parseFloat(raw);
                } else if (f.t === 'select') {
                    obj[f.n] = raw === '' ? null : parseInt(raw, 10);
                } else if (f.t === 'select_str') {
                    obj[f.n] = raw ? raw.trim() : '';
                } else {`;

code = code.replace(collectFind, collectRep);

fs.writeFileSync(file, code, 'utf8');
