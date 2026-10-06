
    (function () {
        var recordModalEl = document.getElementById('recordModal');
        if (recordModalEl) {
            document.body.appendChild(recordModalEl);
        }

        var URLS = {
            list: '@Url.Action("List", "Passport")',
            save: '@Url.Action("Save", "Passport")',
            del: '@Url.Action("Delete", "Passport")'
        };

        var HAT = { 0: 'İç Hat', 1: 'Dış Hat' };
        var YON = { 0: 'Gelen', 1: 'Giden' };

        var fmtNum = function (v) { return (v == null || v === '') ? '-' : Number(v).toLocaleString('tr-TR'); };
        var fmtDate = function (v) {
            if (!v) return '-';
            var s = String(v).split('T')[0];
            var p = s.split('-');
            if (p.length === 3 && p[0].length === 4 && p[1].length === 2 && p[2].length === 2) {
                return p[2] + '.' + p[1] + '.' + p[0];
            }
            return String(v);
        };

        var TYPES = {
            gunluk: {
                title: 'Günlük Yolcu', search: false, year: true, icon: 'bi-people-fill',
                fields: [
                    { n: 'Tarih', l: 'Tarih', t: 'date', req: true, w: 3 },
                    { n: 'Yon', l: 'Yön', t: 'select', opts: YON, w: 2 },
                    { n: 'HatTuru', l: 'Hat Türü', t: 'select', opts: HAT, w: 2 },
                    { n: 'GunlukYolcuSayisi', l: 'Günlük Yolcu Sayısı', t: 'int', req: true, w: 3, ph: '0' },
                    { n: 'UcakSayisi', l: 'Uçak Sayısı', t: 'int', w: 2, ph: '0' }
                ],
                cols: [
                    ['Tarih', 'Tarih', fmtDate],
                    ['Yön', 'Yon', function (v) { return '<span class="badge ' + (v == 0 ? 'bg-info text-dark' : 'bg-warning text-dark') + '">' + (YON[v] || '-') + '</span>'; }],
                    ['Hat', 'HatTuru', function (v) { return '<span class="badge bg-secondary">' + (HAT[v] || '-') + '</span>'; }],
                    ['Yolcu Sayısı', 'GunlukYolcuSayisi', fmtNum],
                    ['Uçak Sayısı', 'UcakSayisi', fmtNum]
                ]
            },
            inad: {
                title: 'İnad Yolcular', search: true, year: true, icon: 'bi-person-x-fill',
                fields: [
                    { n: 'SiraNo', l: 'Sıra No', t: 'int', w: 2, ph: 'Örn: 1' },
                    { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 2 },
                    { n: 'AdSoyad', l: 'Adı Soyadı', t: 'text', req: true, max: 200, w: 4, ph: 'Yolcunun adı ve soyadı' },
                    { n: 'Uyruk', l: 'Uyruk', t: 'text', max: 100, w: 2, ph: 'Örn: IRN, IRQ, DEU' },
                    { n: 'DogumTarihi', l: 'Doğum Tarihi', t: 'date', w: 2 },
                    { n: 'PasaportNo', l: 'Pasaport No', t: 'text', max: 50, w: 3, ph: 'Pasaport / Belge No' },
                    { n: 'GelisTarihi', l: 'Geliş Tarihi', t: 'date', w: 3 },
                    { n: 'GidisTarihi', l: 'Gidiş Tarihi', t: 'date', w: 3 },
                    { n: 'HavayoluSirketi', l: 'Havayolu Şirketi', t: 'text', max: 200, w: 3, ph: 'Örn: THY, Pegasus' },
                    { n: 'GeldigiUlke', l: 'Geldiği Ülke', t: 'text', max: 100, w: 3, ph: 'Ülke adı' },
                    { n: 'GittigiUlke', l: 'Gittiği Ülke', t: 'text', max: 100, w: 3, ph: 'Gönderildiği ülke' },
                    { n: 'InadGerekcesi', l: 'İnad Gerekçesi', t: 'text', max: 500, w: 6, ph: 'Gerekçe kodu veya detayı' },
                    { n: 'Aciklamalar', l: 'Ek Açıklamalar', t: 'textarea', max: 1000, w: 12, ph: 'Varsa diğer notlar...' }
                ],
                cols: [
                    ['Sıra', 'SiraNo', function(v) { return '<span class="text-muted fw-semibold">#' + escapeHtml(v || '-') + '</span>'; }],
                    ['İşlem Tarihi', 'Tarih', fmtDate],
                    ['Adı Soyadı', 'AdSoyad', function(v) { return '<span class="fw-bold text-dark"><i class="bi bi-person me-1 text-secondary"></i>' + escapeHtml(v || '-') + '</span>'; }],
                    ['Uyruk', 'Uyruk', function(v) { return v ? '<span class="badge bg-light text-dark border">' + escapeHtml(v) + '</span>' : '-'; }],
                    ['Pasaport No', 'PasaportNo', function(v) { return v ? '<span class="badge bg-light text-primary border code-tag">' + escapeHtml(v) + '</span>' : '-'; }],
                    ['Geliş / Gidiş', 'GelisTarihi', function(v, row) {
                        return '<div class="small"><span class="text-success"><i class="bi bi-box-arrow-in-down-right"></i> ' + escapeHtml(fmtDate(row.GelisTarihi)) + '</span><br>' +
                               '<span class="text-danger"><i class="bi bi-box-arrow-up-right"></i> ' + escapeHtml(fmtDate(row.GidisTarihi)) + '</span></div>';
                    }],
                    ['Güzergah', 'GeldigiUlke', function(v, row) {
                        return '<span class="small">' + escapeHtml(row.GeldigiUlke || '-') + ' <i class="bi bi-arrow-right text-muted mx-1"></i> ' + escapeHtml(row.GittigiUlke || '-') + '</span>';
                    }],
                    ['Havayolu', 'HavayoluSirketi', function(v) { return v ? '<span class="small text-muted"><i class="bi bi-airplane me-1"></i>' + escapeHtml(v) + '</span>' : '-'; }],
                    ['Gerekçe', 'InadGerekcesi', function(v) {
                        return v ? '<span class="badge bg-warning bg-opacity-25 text-dark border border-warning" title="' + escapeHtml(v) + '">' + escapeHtml(v) + '</span>' : '-';
                    }]
                ]
            },
            tahdit: {
                title: 'Tahdit Kayıtları', search: true, year: true, icon: 'bi-shield-lock-fill',
                fields: [
                    { n: 'Tarih', l: 'İşlem Tarihi', t: 'date', req: true, w: 3 },
                    { n: 'AdSoyad', l: 'Adı Soyadı', t: 'text', req: true, max: 200, w: 4, ph: 'Kişi adı ve soyadı' },
                    { n: 'Uyruk', l: 'Uyruk', t: 'text', max: 100, w: 2, ph: 'Örn: RUS, UKR' },
                    { n: 'DogumTarihi', l: 'Doğum Tarihi', t: 'date', w: 3 },
                    { n: 'PasaportVeyaKimlikNo', l: 'Pasaport / Kimlik No', t: 'text', max: 100, w: 4, ph: 'Belge numarası' },
                    { n: 'TahditKodu', l: 'Tahdit Kodu', t: 'text', req: true, max: 20, w: 3, ph: 'Örn: V-84, C-101, G-87' },
                    { n: 'Neden', l: 'Tahdit Nedeni ve Açıklama', t: 'textarea', max: 500, w: 12, ph: 'Tahdit gerekçesi, mahkeme kararı veya ilgili şerh detayı...' }
                ],
                cols: [
                    ['Tarih', 'Tarih', fmtDate],
                    ['Adı Soyadı', 'AdSoyad', function(v) { return '<span class="fw-bold text-dark"><i class="bi bi-shield-shaded me-1 text-danger"></i>' + escapeHtml(v || '-') + '</span>'; }],
                    ['Uyruk', 'Uyruk', function(v) { return v ? '<span class="badge bg-light text-dark border">' + escapeHtml(v) + '</span>' : '-'; }],
                    ['Pasaport / Kimlik No', 'PasaportVeyaKimlikNo', function(v) { return v ? '<span class="badge bg-light text-secondary border code-tag">' + escapeHtml(v) + '</span>' : '-'; }],
                    ['Tahdit Kodu', 'TahditKodu', function(v) {
                        return '<span class="badge bg-danger text-white px-2 py-1 shadow-sm"><i class="bi bi-exclamation-triangle-fill me-1"></i>' + escapeHtml(v || '-') + '</span>';
                    }],
                    ['Tahdit Nedeni', 'Neden', function(v) {
                        return v ? '<span class="small text-muted" title="' + escapeHtml(v) + '">' + escapeHtml(v.length > 60 ? v.substring(0, 60) + '...' : v) + '</span>' : '-';
                    }]
                ]
            }
        };

        var current = 'gunluk';
        var editingId = 0;
        var rows = [];

        // Sayfalama değişkenleri
        var currentPage = 1;
        var pageSize = 15; // Sayfada gösterilecek kayıt sayısı

        function $(id) { return document.getElementById(id); }

        function escapeHtml(value) {
            return String(value == null ? '' : value)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');
        }

        function token() {
            var input = document.querySelector('#ppApp input[name="__RequestVerificationToken"]');
            return input ? input.value : '';
        }

        function showMsg(msg, ok, inModal) {
            var box = inModal ? $('modalAlertBox') : $('alertBox');
            if (!box) box = $('alertBox');
            box.innerHTML = '';

            var d = document.createElement('div');
            d.className = 'alert alert-' + (ok ? 'success' : 'danger') +
                ' alert-dismissible fade show d-flex align-items-center shadow-sm';

            var icon = document.createElement('i');
            icon.className = 'bi ' +
                (ok ? 'bi-check-circle-fill text-success' : 'bi-exclamation-triangle-fill text-danger') +
                ' fs-5 me-2';

            var content = document.createElement('div');
            content.innerHTML = msg == null ? '' : String(msg);

            var close = document.createElement('button');
            close.type = 'button';
            close.className = 'btn-close';
            close.setAttribute('data-bs-dismiss', 'alert');
            close.setAttribute('aria-label', 'Kapat');

            d.appendChild(icon);
            d.appendChild(content);
            d.appendChild(close);
            box.appendChild(d);

            setTimeout(function () {
                if (d.parentNode) {
                    d.classList.remove('show');
                    setTimeout(function () {
                        if (d.parentNode) d.remove();
                    }, 200);
                }
            }, 6000);
        }

        function post(url, params) {
            var body = new URLSearchParams();
            var csrf = token();

            if (csrf) {
                body.append('__RequestVerificationToken', csrf);
            }

            Object.keys(params || {}).forEach(function (k) {
                body.append(k, params[k] == null ? '' : params[k]);
            });

            return fetch(url, {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: body.toString()
            }).then(function (response) {
                return response.text().then(function (text) {
                    var data = null;
                    try {
                        data = text ? JSON.parse(text) : null;
                    } catch (e) {
                        throw new Error('Sunucudan geçersiz bir yanıt alındı.');
                    }
                    if (!response.ok) {
                        throw new Error((data && data.message) || 'Sunucu isteği işleyemedi. (HTTP ' + response.status + ')');
                    }
                    return data || { success: false, message: 'Sunucudan boş yanıt alındı.' };
                });
            });
        }

        function buildForm() {
            var cfg = TYPES[current];
            $('formTitle').textContent = (editingId ? 'Kaydı Düzenle: ' : 'Yeni Kayıt: ') + cfg.title;
            $('formHeaderIcon').innerHTML = '<i class="bi ' + (cfg.icon || 'bi-pencil-square') + '"></i>';
            $('saveBtn').innerHTML = editingId ? '<i class="bi bi-save me-1"></i> Güncelle' : '<i class="bi bi-check-lg me-1"></i> Kaydet';
            $('cancelEditBtn').style.display = editingId ? 'inline-block' : 'none';

            var wrap = $('formFields');
            wrap.innerHTML = '';
            cfg.fields.forEach(function (f) {
                var col = document.createElement('div');
                col.className = 'col-md-' + (f.w || 3);

                var lb = document.createElement('label');
                lb.className = 'form-label small fw-semibold text-secondary mb-1';
                lb.innerHTML = f.l + (f.req ? ' <span class="text-danger">*</span>' : '');

                var el;
                if (f.t === 'select') {
                    el = document.createElement('select');
                    el.className = 'form-select form-select-sm';
                    Object.keys(f.opts).forEach(function (k) {
                        var o = document.createElement('option');
                        o.value = k;
                        o.textContent = f.opts[k];
                        el.appendChild(o);
                    });
                } else if (f.t === 'textarea') {
                    el = document.createElement('textarea');
                    el.className = 'form-control form-control-sm';
                    el.rows = 2;
                } else {
                    el = document.createElement('input');
                    el.className = 'form-control form-control-sm';
                    el.type = (f.t === 'int' || f.t === 'dec') ? 'number' : (f.t === 'date' ? 'date' : 'text');
                    if (f.t === 'int') { el.min = '0'; el.step = '1'; }
                    if (f.t === 'dec') { el.min = '0'; el.step = '0.01'; }
                }

                if (f.ph) el.placeholder = f.ph;
                if (f.max) el.maxLength = f.max;
                el.id = 'f_' + f.n;

                col.appendChild(lb);
                col.appendChild(el);
                wrap.appendChild(col);
            });

            $('searchWrap').style.display = cfg.search ? '' : 'none';
            $('wrapFilterYear').style.display = cfg.year ? '' : 'none';
        }

        function resetForm() {
            editingId = 0;
            buildForm();
        }

        function fillForm(row) {
            editingId = row.Id;
            buildForm();
            TYPES[current].fields.forEach(function (f) {
                var el = $('f_' + f.n);
                var v = row[f.n];
                if (v != null) {
                    if (f.t === 'date' && typeof v === 'string') {
                        el.value = v.split('T')[0];
                    } else {
                        el.value = v;
                    }
                } else {
                    el.value = '';
                }
            });
            bootstrap.Modal.getOrCreateInstance(document.getElementById('recordModal')).show();
        }

        function collect() {
            var obj = { Id: editingId };
            TYPES[current].fields.forEach(function (f) {
                var el = $('f_' + f.n);
                var raw = el ? el.value : '';

                if (f.t === 'int') {
                    obj[f.n] = raw === '' ? 0 : parseInt(raw, 10);
                } else if (f.t === 'dec') {
                    obj[f.n] = raw === '' ? 0 : parseFloat(raw);
                } else if (f.t === 'select') {
                    obj[f.n] = raw === '' ? null : parseInt(raw, 10);
                } else {
                    obj[f.n] = raw ? raw.trim() : '';
                }
            });
            return obj;
        }

        window.editDuplicate = function(id) {
            // Check if it's in the current rows
            var row = rows.find(function(r) { return r.Id === id; });
            if (row) {
                fillForm(row);
                var box = $('modalAlertBox');
                if (box) box.innerHTML = ''; // clear error
            } else {
                // Try fetching it from server
                post('@Url.Action("Get", "Passport")', { type: current, id: id })
                    .then(function(r) {
                        if (r.success && r.data) {
                            fillForm(r.data);
                            var box = $('modalAlertBox');
                            if (box) box.innerHTML = ''; // clear error
                        } else {
                            showMsg('Kayıt bulunamadı.', false, true);
                        }
                    })
                    .catch(function() {
                        showMsg('Kayıt getirilirken hata oluştu.', false, true);
                    });
            }
        };

        function save() {
            var cfg = TYPES[current];

            for (var i = 0; i < cfg.fields.length; i++) {
                var f = cfg.fields[i];
                var el = $('f_' + f.n);
                var value = el ? String(el.value || '').trim() : '';

                if (f.req && value === '') {
                    showMsg(f.l + ' alanı zorunludur.', false, true);
                    if (el) el.focus();
                    return;
                }
                if (f.t === 'int' && value !== '') {
                    var intValue = Number(value);
                    if (!Number.isInteger(intValue) || intValue < 0) {
                        showMsg(f.l + ' alanına 0 veya daha büyük bir tam sayı giriniz.', false, true);
                        el.focus();
                        return;
                    }
                }
            }

            var payload = collect();
            var btn = $('saveBtn');
            var oldHtml = btn.innerHTML;

            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Kaydediliyor...';

            post(URLS.save, {
                type: current,
                payload: JSON.stringify(payload)
            })
                .then(function (r) {
                    if (!r || r.success !== true) {
                        if (r && r.duplicateId) {
                            var errMsg = (r.message || 'Kayıt zaten var.') + ' <button type="button" class="btn btn-sm btn-warning ms-3" onclick="editDuplicate(' + r.duplicateId + ')">Mevcut Kaydı Düzenle</button>';
                            showMsg(errMsg, false, true);
                        } else {
                            throw new Error((r && r.message) || 'Kayıt işlemi gerçekleştirilemedi.');
                        }
                        return;
                    }
                    showMsg(editingId ? 'Kayıt güncellendi.' : 'Kayıt başarıyla oluşturuldu.', true);
                    resetForm();
                    var modalElement = document.getElementById('recordModal');
                    var modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) modal.hide();
                    load();
                })
                .catch(function (err) {
                    showMsg(err && err.message ? err.message : 'Kayıt sırasında sunucu hatası oluştu.', false, true);
                })
                .then(function () {
                    btn.disabled = false;
                    btn.innerHTML = oldHtml;
                });
        }

        function remove(row) {
            if (!confirm('Bu kaydı silmek istediğinize emin misiniz?')) return;
            post(URLS.del, { type: current, id: row.Id })
                .then(function (r) {
                    showMsg(r.success ? 'Kayıt başarıyla silindi.' : r.message, r.success);
                    if (r.success) {
                        if (editingId === row.Id) resetForm();
                        load();
                    }
                })
                .catch(function () { showMsg('Silme işlemi sırasında sunucu hatası oluştu.', false); });
        }

        function renderPagination(totalRows) {
            var container = $('paginationContainer');
            if (totalRows <= pageSize) {
                container.innerHTML = '';
                return;
            }

            var totalPages = Math.ceil(totalRows / pageSize);
            var html = '<ul class="pagination pagination-sm mb-0 shadow-sm">';

            // Önceki butonu
            html += '<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '">';
            html += '<a class="page-link" href="#" data-page="' + (currentPage - 1) + '"><i class="bi bi-chevron-left"></i></a></li>';

            for (var i = 1; i <= totalPages; i++) {
                // Sadece mevcut sayfanın etrafındaki sayfaları ve ilk/son sayfaları göster
                if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                    html += '<li class="page-item ' + (currentPage === i ? 'active' : '') + '">';
                    html += '<a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>';
                } else if (i === currentPage - 3 || i === currentPage + 3) {
                    html += '<li class="page-item disabled"><a class="page-link" href="#">...</a></li>';
                }
            }

            // Sonraki butonu
            html += '<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '">';
            html += '<a class="page-link" href="#" data-page="' + (currentPage + 1) + '"><i class="bi bi-chevron-right"></i></a></li>';

            html += '</ul>';
            container.innerHTML = html;
        }

        function render() {
            var cfg = TYPES[current];
            var head = $('listHead');
            head.innerHTML = '';
            cfg.cols.forEach(function (c) {
                var th = document.createElement('th');
                th.textContent = c[0];
                head.appendChild(th);
            });
            var thAct = document.createElement('th');
            thAct.className = 'text-end px-3';
            thAct.textContent = 'İşlemler';
            head.appendChild(thAct);

            var body = $('listBody');
            body.innerHTML = '';
            if (!rows.length) {
                var tr0 = document.createElement('tr');
                var td0 = document.createElement('td');
                td0.colSpan = cfg.cols.length + 1;
                td0.className = 'text-center py-5 text-muted';
                td0.innerHTML = '<i class="bi bi-inbox fs-2 d-block mb-2 text-secondary"></i>Kayıt bulunamadı.';
                tr0.appendChild(td0);
                body.appendChild(tr0);

                $('listInfo').innerHTML = '';
                $('paginationContainer').innerHTML = '';
            } else {
                // Sayfalama için satırları dilimle (Slice)
                var totalPages = Math.ceil(rows.length / pageSize);
                if (currentPage > totalPages) currentPage = totalPages;
                if (currentPage < 1) currentPage = 1;

                var startIndex = (currentPage - 1) * pageSize;
                var pagedRows = rows.slice(startIndex, startIndex + pageSize);

                pagedRows.forEach(function (row) {
                    var tr = document.createElement('tr');
                    cfg.cols.forEach(function (c) {
                        var td = document.createElement('td');
                        var v = row[c[1]];
                        td.innerHTML = c[2] ? c[2](v, row) : escapeHtml(v == null ? '-' : v);
                        tr.appendChild(td);
                    });

                    var act = document.createElement('td');
                    act.className = 'text-end text-nowrap px-3';

                    var btnEdit = document.createElement('button');
                    btnEdit.className = 'btn btn-sm btn-outline-primary me-1 rounded-pill px-2';
                    btnEdit.innerHTML = '<i class="bi bi-pencil"></i>';
                    btnEdit.title = 'Düzenle';
                    btnEdit.onclick = function () { fillForm(row); };

                    var btnDel = document.createElement('button');
                    btnDel.className = 'btn btn-sm btn-outline-danger rounded-pill px-2';
                    btnDel.innerHTML = '<i class="bi bi-trash"></i>';
                    btnDel.title = 'Sil';
                    btnDel.onclick = function () { remove(row); };

                    act.appendChild(btnEdit);
                    act.appendChild(btnDel);
                    tr.appendChild(act);
                    body.appendChild(tr);
                });

                // Bilgi ve Pagination Render
                var endRow = Math.min(startIndex + pageSize, rows.length);
                $('listInfo').innerHTML = '<i class="bi bi-info-circle me-1"></i> Toplam <strong>' + rows.length + '</strong> kayıttan <strong>' + (startIndex + 1) + '-' + endRow + '</strong> arası gösteriliyor.';
                renderPagination(rows.length);
            }
        }

        function load() {
            var cfg = TYPES[current];
            var qs = 'type=' + encodeURIComponent(current);
            var y = $('filterYear').value;
            if (cfg.year && y) qs += '&year=' + encodeURIComponent(y);
            var s = $('filterSearch').value;
            if (cfg.search && s) qs += '&search=' + encodeURIComponent(s);

            var body = $('listBody');
            body.innerHTML = '<tr><td colspan="' + (cfg.cols.length + 1) + '" class="text-center py-4 text-muted"><div class="spinner-border spinner-border-sm me-2 text-primary" role="status"></div>Yükleniyor...</td></tr>';

            fetch(URLS.list + '?' + qs, { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (r) {
                    if (!r.success) {
                        showMsg(r.message || 'Veriler alınamadı.', false);
                        rows = [];
                    } else {
                        rows = r.data || [];
                    }
                    currentPage = 1; // Yeni veri yüklendiğinde 1. sayfaya dön
                    render();
                })
                .catch(function () {
                    showMsg('Veriler yüklenirken sunucuya ulaşılamadı.', false);
                    rows = [];
                    render();
                });
        }

        $('ppTabs').addEventListener('click', function (e) {
            var btn = e.target.closest('button.nav-link');
            if (!btn) return;
            var t = btn.getAttribute('data-type');
            if (!t) return;
            e.preventDefault();

            document.querySelectorAll('#ppTabs .nav-link').forEach(function (a) { a.classList.remove('active'); });
            btn.classList.add('active');
            current = t;

            $('filterYear').value = '';
            $('filterSearch').value = '';
            resetForm();
            load();
        });

        // Sayfalama tıklama olayları
        $('paginationContainer').addEventListener('click', function(e) {
            e.preventDefault();
            var a = e.target.closest('a.page-link');
            if (!a) return;

            var p = parseInt(a.getAttribute('data-page'), 10);
            if (!isNaN(p) && p !== currentPage) {
                currentPage = p;
                render();
            }
        });

        $('saveBtn').onclick = save;
        $('resetBtn').onclick = resetForm;
        $('cancelEditBtn').onclick = resetForm;

        var recordModal = document.getElementById('recordModal');
        if (recordModal) {
            recordModal.addEventListener('hidden.bs.modal', function () { resetForm(); });
        }

        var btnNewRecord = $('btnNewRecord');
        if (btnNewRecord) btnNewRecord.onclick = resetForm;

        $('filterBtn').onclick = load;
        $('clearFilterBtn').onclick = function () {
            $('filterYear').value = '';
            $('filterSearch').value = '';
            load();
        };

        $('filterSearch').addEventListener('keydown', function (e) { if (e.key === 'Enter') load(); });
        $('filterYear').addEventListener('keydown', function (e) { if (e.key === 'Enter') load(); });

        var yearSelect = $('filterYear');
        if (yearSelect) {
            var currentYear = new Date().getFullYear();
            for (var y = currentYear; y >= 2020; y--) {
                var opt = document.createElement('option');
                opt.value = y;
                opt.textContent = y;
                yearSelect.appendChild(opt);
            }
            yearSelect.addEventListener('change', load);
        }

        resetForm();
        load();
    })();
    
