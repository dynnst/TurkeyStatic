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

// 2. Add modalAlertBox
code = code.replace('<div class="modal-body p-4 bg-white">', '<div class="modal-body p-4 bg-white">\n                    <div id="modalAlertBox"></div>');

// 3. showMsg inModal signature
code = code.replace('function showMsg(msg, ok) {', 'function showMsg(msg, ok, inModal) {');
code = code.replace("var box = $('alertBox');", "var box = inModal ? $('modalAlertBox') : $('alertBox');\n            if (!box) box = $('alertBox');");
code = code.replace("content.textContent = msg == null ? '' : String(msg);", "content.innerHTML = msg == null ? '' : String(msg);");

// 4. Update save() validations
const startIdx = code.indexOf('function save() {');
const endIdx = code.indexOf('function remove(row)', startIdx);

if (startIdx !== -1 && endIdx !== -1) {
    let before = code.substring(0, startIdx);
    let saveMethod = code.substring(startIdx, endIdx);
    let after = code.substring(endIdx);
    
    saveMethod = saveMethod.replace(/showMsg\(([^,]+),\s*false\);/g, "showMsg($1, false, true);");
    saveMethod = saveMethod.replace(/showMsg\(([^,]+),\s*true\);/g, "showMsg($1, true, true);");
    
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

    saveMethod = saveMethod.replace(/post\(URLS\.save[\s\S]*?\.catch\(function[^\)]*\)[\s\S]*?\}\)/, fixPost);

    code = before + saveMethod + after;
}

fs.writeFileSync(file, code, 'utf8');
