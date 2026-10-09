// Helper fonksiyonlar
function formatDate(date) {
    if (typeof date === 'string') {
        date = new Date(date);
    }
    return date.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function formatNumber(number) {
    return new Intl.NumberFormat('tr-TR').format(number);
}

function showLoader() {
    const el = document.getElementById('loader');
    if (el) el.style.display = 'block';
}

function hideLoader() {
    const el = document.getElementById('loader');
    if (el) el.style.display = 'none';
}

function showError(message) {
    let container = document.getElementById('error-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'error-container';
        const main = document.querySelector('.container-fluid') || document.body;
        main.insertBefore(container, main.firstChild);
    }
    const alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show my-3" role="alert">
            <i class="bi bi-exclamation-triangle-fill me-2"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    container.innerHTML = alertHtml;
    
    setTimeout(() => {
        const alert = container.querySelector('.alert');
        if (alert) alert.remove();
    }, 6000);
}

function showSuccess(message) {
    let container = document.getElementById('error-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'error-container';
        const main = document.querySelector('.container-fluid') || document.body;
        main.insertBefore(container, main.firstChild);
    }
    const alertHtml = `
        <div class="alert alert-success alert-dismissible fade show my-3" role="alert">
            <i class="bi bi-check-circle-fill me-2"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    container.innerHTML = alertHtml;
    
    setTimeout(() => {
        const alert = container.querySelector('.alert');
        if (alert) alert.remove();
    }, 4000);
}
