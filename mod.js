const fs = require('fs');
const file = 'C:/Users/fb/Downloads/Istatistik/Istatistik/Istatistik/Views/Passport/Index.cshtml';
let code = fs.readFileSync(file, 'utf8');

// 1. Add editDuplicate
const editDuplicateStr = `
        window.editDuplicate = function(id) {
            var row = rows.find(function(r) { return r.Id === id; });
            if (row) {
                fillForm(row);
                var box = $('modalAlertBox');
                if (box) box.innerHTML = '';
            } else {
                post('@Url.Action("Get", "Passport")', { type: current, id: id })
                    .then(function(r) {
                        if (r.success && r.data) {
                            fillForm(r.data);
                            var box = $('modalAlertBox');
                            if (box) box.innerHTML = '';
                        } else {
                            showMsg('Kayıt bulunamadı.', false, true);
                        }
                    })
                    .catch(function() {
                        showMsg('Kayıt getirilirken hata oluştu.', false, true);
                    });
            }
        };

        function save() {`;

code = code.replace("function save() {", editDuplicateStr);

// 2. Fix showMsg calls inside save() validation
code = code.replace(/showMsg\(([^,]+),\s*false\);/g, "showMsg($1, false, true);");
code = code.replace(/showMsg\(([^,]+),\s*true\);/g, "showMsg($1, true, true);");

// 3. Fix the post().then() block for save()
const oldPost = `post(URLS.save, {
                type: current,
                payload: JSON.stringify(payload)
            })
                .then(function (r) {
                    if (!r || r.success !== true) throw new Error((r && r.message) || 'Kayıt işlemi gerçekleştirilemedi.');
                    showMsg(editingId ? 'Kayıt güncellendi.' : 'Kayıt başarıyla oluşturuldu.', true, true);
                    resetForm();
                    var modalElement = document.getElementById('recordModal');
                    var modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) modal.hide();
                    load();
                })
                .catch(function (err) {
                    showMsg(err && err.message ? err.message : 'Kayıt sırasında sunucu hatası oluştu.', false, true);
                })`;

// The actual code might have weird characters in the strings, so we use regex.
const fixPost = `post(URLS.save, {
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
                    showMsg(editingId ? 'Kayıt güncellendi.' : 'Kayıt başarıyla oluşturuldu.', true, true);
                    resetForm();
                    var modalElement = document.getElementById('recordModal');
                    var modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) modal.hide();
                    load();
                })
                .catch(function (err) {
                    showMsg(err && err.message ? err.message : 'Kayıt sırasında sunucu hatası oluştu.', false, true);
                })`;

code = code.replace(/post\(URLS\.save[\s\S]*?\.catch\(function[^\)]*\)[\s\S]*?\}\)/, fixPost);

fs.writeFileSync(file, code, 'utf8');
