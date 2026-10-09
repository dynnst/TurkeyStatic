// Main.js - Raporlama ve Görselleştirme Orkestrasyonu

let currentFilters = {
    dataType: 'yolcuucak',
    startDate: '',
    endDate: '',
    periodType: 'monthly'
};

document.addEventListener('DOMContentLoaded', function () {
    initFiltersAndDates();
    bindEventListeners();

    // Index sayfasındaysak ilk grafik yüklemesini başlat
    const isIndexPage = !!document.getElementById('startDate');
    if (isIndexPage) {
        loadChartData();
    }
});

// ── Başlangıç Değerleri ve Tarih Ayarları ─────────────────────────────────────

function initFiltersAndDates() {
    const today = new Date();
    const oneMonthAgo = new Date(today);
    oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);

    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    const dataTypeSelect = document.getElementById('dataType');
    const periodTypeSelect = document.getElementById('periodType');

    // Kaydedilmiş filtreleri geri yükle (Requirement 9.5)
    const saved = getSavedFilters();
    if (saved && startDateInput && endDateInput) {
        if (saved.startDate) startDateInput.value = saved.startDate;
        if (saved.endDate) endDateInput.value = saved.endDate;
        if (saved.dataType && dataTypeSelect) dataTypeSelect.value = saved.dataType;
        if (saved.periodType && periodTypeSelect) periodTypeSelect.value = saved.periodType;
    } else {
        if (startDateInput) startDateInput.valueAsDate = oneMonthAgo;
        if (endDateInput) endDateInput.valueAsDate = today;
    }

    // Karşılaştırma Dönem 1 (Baz Dönem: 1 yıl önce)
    const p1Start = new Date(oneMonthAgo);
    p1Start.setFullYear(p1Start.getFullYear() - 1);
    const p1End = new Date(today);
    p1End.setFullYear(p1End.getFullYear() - 1);

    const p1StartInput = document.getElementById('p1StartDate');
    const p1EndInput = document.getElementById('p1EndDate');
    const p2StartInput = document.getElementById('p2StartDate');
    const p2EndInput = document.getElementById('p2EndDate');

    if (p1StartInput) p1StartInput.valueAsDate = p1Start;
    if (p1EndInput) p1EndInput.valueAsDate = p1End;

    // Karşılaştırma Dönem 2 (Kıyas Dönemi: son 1 ay)
    if (p2StartInput) p2StartInput.valueAsDate = oneMonthAgo;
    if (p2EndInput) p2EndInput.valueAsDate = today;

    // Dönem 1 değiştikçe Dönem 2'yi +1 yıl olarak senkronize et
    if (p1StartInput) {
        p1StartInput.addEventListener('change', function () {
            syncPeriod2Date('p1StartDate', 'p2StartDate');
        });
    }
    if (p1EndInput) {
        p1EndInput.addEventListener('change', function () {
            syncPeriod2Date('p1EndDate', 'p2EndDate');
        });
    }
}

// ── Event Listener Bağlantıları ──────────────────────────────────────────────

function bindEventListeners() {
    bind('btnFilter', 'click', loadChartData);

    const chartTypeEl = document.getElementById('chartType');
    if (chartTypeEl) {
        chartTypeEl.addEventListener('change', function () {
            ChartModule.changeType(this.value);
        });
    }

    bind('btnDownloadPng', 'click', () => ChartModule.download('png'));
    bind('btnDownloadJpeg', 'click', () => ChartModule.download('jpeg'));
    bind('btnDownloadChartExcel', 'click', exportChartExcel);

    const pageSizeEl = document.getElementById('pageSize');
    if (pageSizeEl) {
        pageSizeEl.addEventListener('change', function () {
            TableModule.setPageSize(parseInt(this.value));
            loadTableData(1);
        });
    }

    bind('btnExportCsv', 'click', exportCsv);
    bind('btnExportExcel', 'click', exportExcel);

    // Karşılaştırma
    bind('btnCompare', 'click', loadComparisonData);
    bind('btnDownloadCompPng', 'click', () => ComparisonModule.downloadChart('png'));
    bind('btnDownloadCompJpeg', 'click', () => ComparisonModule.downloadChart('jpeg'));
    bind('btnDownloadCompExcel', 'click', exportComparisonExcel);

    // Sekme Değişikliği
    document.querySelectorAll('[data-bs-toggle="tab"]').forEach(tab => {
        tab.addEventListener('shown.bs.tab', function (e) {
            const target = e.target.getAttribute('data-bs-target');
            if (target === '#table-tab') {
                loadTableData(1);
            }
        });
    });

    // Popover'lar (Bootstrap)
    if (typeof bootstrap !== 'undefined' && bootstrap.Popover) {
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.forEach(el => new bootstrap.Popover(el));
    }
}

function bind(id, event, handler) {
    const el = document.getElementById(id);
    if (el) {
        el.addEventListener(event, handler);
    }
}

// ── Validasyon Fonksiyonları (Requirements 2.8, 2.9, 2.10, 5.2) ──────────────

function validateDateRange(startDateStr, endDateStr) {
    if (!startDateStr || !endDateStr) {
        showError('Lütfen başlangıç ve bitiş tarihlerini seçin');
        return false;
    }

    const start = new Date(startDateStr);
    const end = new Date(endDateStr);
    const today = new Date();
    today.setHours(23, 59, 59, 999);

    if (end < start) {
        showError('Bitiş tarihi başlangıç tarihinden önce olamaz');
        return false;
    }

    if (start > today || end > today) {
        showError('Gelecek tarih seçilemez');
        return false;
    }

    const diffTime = Math.abs(end - start);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    if (diffDays > 1826) { // 5 yıl (yaklaşık 1826 gün)
        showError('Maksimum 5 yıllık zaman aralığı seçebilirsiniz');
        return false;
    }

    return true;
}

function validateComparisonRanges(p1StartStr, p1EndStr, p2StartStr, p2EndStr) {
    if (!p1StartStr || !p1EndStr || !p2StartStr || !p2EndStr) {
        showError('Lütfen her iki dönemin de başlangıç ve bitiş tarihlerini doldurun');
        return false;
    }

    if (!validateDateRange(p1StartStr, p1EndStr)) return false;
    if (!validateDateRange(p2StartStr, p2EndStr)) return false;

    const p1Start = new Date(p1StartStr);
    const p1End = new Date(p1EndStr);
    const p2Start = new Date(p2StartStr);
    const p2End = new Date(p2EndStr);

    // Dönemler örtüşemez kontrolü (Requirement 5.2)
    if (p1Start <= p2End && p1End >= p2Start) {
        showError('Dönemler örtüşemez');
        return false;
    }

    return true;
}

// ── Veri Yükleme ─────────────────────────────────────────────────────────────

async function loadChartData() {
    const dataTypeEl = document.getElementById('dataType');
    const startDateEl = document.getElementById('startDate');
    const endDateEl = document.getElementById('endDate');
    const periodTypeEl = document.getElementById('periodType');

    if (!dataTypeEl || !startDateEl || !endDateEl) return;

    currentFilters.dataType = dataTypeEl.value;
    currentFilters.startDate = startDateEl.value;
    currentFilters.endDate = endDateEl.value;
    currentFilters.periodType = periodTypeEl ? periodTypeEl.value : 'monthly';

    if (!validateDateRange(currentFilters.startDate, currentFilters.endDate)) {
        return;
    }

    // Yolcu/Uçak istatistikleri aylık veya yıllık tutulur
    if (currentFilters.dataType === 'yolcuucak' &&
        (currentFilters.periodType === 'daily' || currentFilters.periodType === 'weekly')) {
        showError('Yolcu/Uçak istatistikleri günlük veya haftalık gösterilemez. Lütfen Aylık veya Yıllık seçin.');
        return;
    }

    saveFilters(currentFilters);

    try {
        const data = await ApiClient.post('/Reports/GetChartData', currentFilters);
        if (!data || !data.DataPoints || data.DataPoints.length === 0) {
            showError('Seçilen tarih aralığında veri bulunamadı');
            ChartModule.destroy();
        } else {
            const chartType = document.getElementById('chartType') ? document.getElementById('chartType').value : 'line';
            ChartModule.create('reportChart', data.DataPoints, chartType);
            showSuccess('Veriler güncellendi');
        }

        const tablePane = document.getElementById('table-tab');
        if (tablePane && tablePane.classList.contains('active')) {
            loadTableData(1);
        }
    } catch (error) {
        console.error('Chart load error:', error);
    }
}

async function loadTableData(page = 1, sortBy = null, sortDesc = false) {
    const params = {
        ...currentFilters,
        page: page,
        pageSize: TableModule.getPageSize(),
        sortBy: sortBy,
        sortDesc: sortDesc
    };

    try {
        const data = await ApiClient.post('/Reports/GetTableData', params);
        TableModule.render('tableContainer', data);
    } catch (error) {
        console.error('Table load error:', error);
    }
}

window.loadTableData = loadTableData;

async function loadComparisonData() {
    const dataTypeEl = document.getElementById('dataType');
    const periodTypeEl = document.getElementById('periodType');
    const p1StartEl = document.getElementById('p1StartDate');
    const p1EndEl = document.getElementById('p1EndDate');
    const p2StartEl = document.getElementById('p2StartDate');
    const p2EndEl = document.getElementById('p2EndDate');

    if (!p1StartEl || !p1EndEl || !p2StartEl || !p2EndEl) return;

    const dataType = dataTypeEl ? dataTypeEl.value : 'gunluk';
    const periodType = periodTypeEl ? periodTypeEl.value : 'monthly';
    const p1Start = p1StartEl.value;
    const p1End = p1EndEl.value;
    const p2Start = p2StartEl.value;
    const p2End = p2EndEl.value;

    if (!validateComparisonRanges(p1Start, p1End, p2Start, p2End)) {
        return;
    }

    try {
        const data = await ApiClient.post('/Reports/GetComparisonData', {
            dataType: dataType,
            period1Start: p1Start,
            period1End: p1End,
            period2Start: p2Start,
            period2End: p2End,
            periodType: periodType
        });

        ComparisonModule.renderTable('comparisonTable', data);
        ComparisonModule.renderChart('comparisonChart', data);

        const resultsEl = document.getElementById('comparisonResults');
        if (resultsEl) resultsEl.style.display = 'block';

        showSuccess('Karşılaştırma analizi tamamlandı');
    } catch (error) {
        console.error('Comparison error:', error);
    }
}

// ── Export İşlemleri ─────────────────────────────────────────────────────────

function exportCsv() {
    ApiClient.download('/Reports/ExportCsv', currentFilters);
}

function exportExcel() {
    ApiClient.download('/Reports/ExportExcel', currentFilters);
}

function exportChartExcel() {
    const base64 = ChartModule.getBase64Image('png');
    const selectEl = document.getElementById('dataType');
    const title = selectEl ? selectEl.options[selectEl.selectedIndex].text : currentFilters.dataType;

    ApiClient.download('/Reports/ExportChartExcel', {
        ...currentFilters,
        chartImageBase64: base64,
        chartTitle: title
    });
}

function exportComparisonExcel() {
    const dataTypeEl = document.getElementById('dataType');
    const periodTypeEl = document.getElementById('periodType');
    const p1StartEl = document.getElementById('p1StartDate');
    const p1EndEl = document.getElementById('p1EndDate');
    const p2StartEl = document.getElementById('p2StartDate');
    const p2EndEl = document.getElementById('p2EndDate');

    if (!p1StartEl || !p1EndEl || !p2StartEl || !p2EndEl) return;

    const base64 = ComparisonModule.getBase64Image('png');

    ApiClient.download('/Reports/ExportComparisonExcel', {
        dataType: dataTypeEl ? dataTypeEl.value : 'gunluk',
        periodType: periodTypeEl ? periodTypeEl.value : 'monthly',
        period1Start: p1StartEl.value,
        period1End: p1EndEl.value,
        period2Start: p2StartEl.value,
        period2End: p2EndEl.value,
        chartImageBase64: base64
    });
}

// ── Yardımcı Fonksiyonlar ───────────────────────────────────────────────────

function syncPeriod2Date(sourceId, targetId) {
    const sourceEl = document.getElementById(sourceId);
    const targetEl = document.getElementById(targetId);
    if (!sourceEl || !targetEl || !sourceEl.value) return;

    const source = new Date(sourceEl.value);
    if (isNaN(source.getTime())) return;

    const target = new Date(source);
    target.setFullYear(target.getFullYear() + 1);

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (target > today) {
        target.setTime(today.getTime());
    }

    targetEl.value = target.toISOString().substring(0, 10);
}

function saveFilters(filters) {
    try {
        localStorage.setItem('reportFilters', JSON.stringify(filters));
    } catch (_) { }
}

function getSavedFilters() {
    try {
        const saved = localStorage.getItem('reportFilters');
        return saved ? JSON.parse(saved) : null;
    } catch (_) {
        return null;
    }
}
