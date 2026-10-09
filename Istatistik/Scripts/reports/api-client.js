// API Client - Anti-forgery token ile AJAX istekleri
const ApiClient = (function() {
    function getAntiForgeryToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }
    
    async function post(url, data) {
        showLoader();
        try {
            const formData = new FormData();
            for (const key in data) {
                if (data[key] !== null && data[key] !== undefined) {
                    formData.append(key, data[key]);
                }
            }
            formData.append('__RequestVerificationToken', getAntiForgeryToken());
            
            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });
            
            if (!response.ok) {
                let errorMsg = '';
                try {
                    const errData = await response.json();
                    if (errData && errData.message) {
                        errorMsg = errData.message;
                    }
                } catch (_) {}

                if (!errorMsg) {
                    if (response.status === 403) {
                        errorMsg = 'Bu işlem için yetkiniz yok';
                    } else if (response.status === 400) {
                        errorMsg = 'Geçersiz parametre veya istek';
                    } else {
                        errorMsg = `İşlem sırasında bir hata oluştu (HTTP ${response.status})`;
                    }
                }
                throw new Error(errorMsg);
            }
            
            const result = await response.json();
            if (!result.success) {
                throw new Error(result.message || 'İşlem başarısız');
            }
            
            return result.data;
        } catch (error) {
            showError(error.message);
            throw error;
        } finally {
            hideLoader();
        }
    }
    
    function downloadFile(url, data) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = url;
        
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = getAntiForgeryToken();
        form.appendChild(tokenInput);
        
        for (const key in data) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = data[key];
            form.appendChild(input);
        }
        
        document.body.appendChild(form);
        form.submit();
        document.body.removeChild(form);
    }
    
    return {
        post: post,
        download: downloadFile
    };
})();
