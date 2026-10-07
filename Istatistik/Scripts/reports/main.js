// Main.js - Orchestration
let currentFilters = {
    dataType: 'yolcuucak',
    startDate: '',
    endDate: '',
    periodType: 'monthly',
    customDays: 10
};

document.addEventListener('DOMContentLoaded', function() {
    // Varsayılan tarihleri ayarla
    const today = new Date();
    const oneMonthAgo = new Date(today);
    oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);

    const sd = document.getElementById('startDate'); if (sd) sd.valueAsDate = oneMonthAgo;
    const ed = document.getElementById('endDate'); if (ed) ed.valueAsDate = today;

    // Dönem 1 başlangıç: bir yıl önce, aynı ay/gün
    const p1Start = new Date(oneMonthAgo);
    p1Start.setFullYear(p1Start.getFullYear() - 1);
    const p1End = new Date(today);
    p1End.setFullYear(p1End.getFullYear() - 1);

    const p1s = document.getElementById('p1StartDate'); if(p1s) p1s.valueAsDate = p1Start;
    const p1e = document.getElementById('p1EndDate'); if(p1e) p1e.valueAsDate = p1End;

    // Dönem 2: aynı ay/gün, bu yıl (bir yıl sonrası)
    const p2s = document.getElementById('p2StartDate'); if(p2s) p2s.valueAsDate = oneMonthAgo;
    const p2e = document.getElementById('p2EndDate'); if (p2e) p2e.valueAsDate = today;

    // Dönem 1 tarih değişince → Dönem 2'yi otomatik güncelle (aynı ay/gün, +1 yıl)
    const p1StartInput = document.getElementById('p1StartDate');
    if (p1StartInput) {
        p1StartInput.addEventListener('change', function() {
            syncPeriod2Date('p1StartDate', 'p2StartDate');
        });
        document.getElementById('p1EndDate').addEventListener('change', function() {
            syncPeriod2Date('p1EndDate', 'p2EndDate');
        });
    }
    
    // Dönem tipi değişikliğinde özel gün sayısını göster/gizle
    const ptSelect = document.getElementById('periodType'); if (ptSelect) ptSelect.addEventListener('change', function() {
        const customContainer = document.getElementById('customPeriodContainer');
        const btnContainer = document.getElementById('btnFilterContainer');
        if (!customContainer || !btnContainer) return;
        if (this.value === 'custom') {
            customContainer.style.display = 'block';
            btnContainer.classList.remove('col-md-3');
            btnContainer.classList.add('col-md-2');
        } else {
            customContainer.style.display = 'none';
            btnContainer.classList.remove('col-md-2');
            btnContainer.classList.add('col-md-3');
        }
    });

    // Sayfa açılışında durumu kontrol et
    const pt = document.getElementById('periodType'); if(pt) pt.dispatchEvent(new Event('change'));
    
    // Event listeners
    const btnFilter = document.getElementById('btnFilter');
    if (btnFilter) btnFilter.addEventListener('click', loadChartData);

    const chartType = document.getElementById('chartType');
    if (chartType) chartType.addEventListener('change', function() { ChartModule.changeType(this.value); });

    const btnDownloadPng = document.getElementById('btnDownloadPng');
    if (btnDownloadPng) btnDownloadPng.addEventListener('click', () => ChartModule.download('png'));

    const btnDownloadJpeg = document.getElementById('btnDownloadJpeg');
    if (btnDownloadJpeg) btnDownloadJpeg.addEventListener('click', () => ChartModule.download('jpeg'));

    const pageSize = document.getElementById('pageSize');
    if (pageSize) pageSize.addEventListener('change', function() {
        TableModule.setPageSize(parseInt(this.value));
        loadTableData(1);
    });

    const btnExportCsv = document.getElementById('btnExportCsv');
    if (btnExportCsv) btnExportCsv.addEventListener('click', exportCsv);

    const btnExportExcel = document.getElementById('btnExportExcel');
    if (btnExportExcel) btnExportExcel.addEventListener('click', exportExcel);

    const btnCompare = document.getElementById('btnCompare');
    if (btnCompare) btnCompare.addEventListener('click', loadComparisonData);
    
    // Tab değişikliği
    document.querySelectorAll('[data-bs-toggle="tab"]').forEach(tab => {
        tab.addEventListener('shown.bs.tab', function(e) {
            const target = e.target.getAttribute('data-bs-target');
            if (target === '#table-tab') {
                loadTableData(1);
            }
        });
    });
    
    // Popover'ları başlat (Uyarı Balonları)
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // İlk yükleme
    loadChartData();
});

async function loadChartData() {
    if (!document.getElementById('startDate')) return;
    currentFilters.dataType = document.getElementById('dataType').value;
    currentFilters.startDate = document.getElementById('startDate').value;
    currentFilters.endDate = document.getElementById('endDate').value;
    currentFilters.periodType = document.getElementById('periodType').value;
    currentFilters.customDays = document.getElementById('customDays') ? document.getElementById('customDays').value : 10;

    if (!currentFilters.startDate || !currentFilters.endDate) {
        showError('Lütfen başlangıç ve bitiş tarihlerini seçin');
        return;
    }

    // YolcuUcak günlük/haftalık/özel desteklenmez
    if (currentFilters.dataType === 'yolcuucak' &&
        (currentFilters.periodType === 'daily' || currentFilters.periodType === 'weekly' || currentFilters.periodType === 'custom')) {
        showError('Yolcu/Uçak istatistikleri için Günlük, Haftalık veya Özel dönem desteklenmez. Lütfen Aylık veya Yıllık seçin.');
        return;
    }

    try {
        const data = await ApiClient.post('/Reports/GetChartData', currentFilters);
        if (!data.DataPoints || data.DataPoints.length === 0) {
            showError('Seçilen tarih aralığında veri bulunamadı');
            ChartModule.destroy();
            return;
        }
        ChartModule.create('reportChart', data.DataPoints, document.getElementById('chartType').value);
        showSuccess('Grafik güncellendi');
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

// Global olarak erişilebilir olması için window'a ekle
window.loadTableData = loadTableData;

async function loadComparisonData() {
    if (!document.getElementById('p1StartDate')) return;
    const dataType = document.getElementById('dataType').value;
    const periodType = document.getElementById('periodType').value;
    const customDays = document.getElementById('customDays') ? document.getElementById('customDays').value : 10;
    const p1Start = document.getElementById('p1StartDate').value;
    const p1End = document.getElementById('p1EndDate').value;
    const p2Start = document.getElementById('p2StartDate').value;
    const p2End = document.getElementById('p2EndDate').value;
    
    if (!p1Start || !p1End || !p2Start || !p2End) {
        showError('Lütfen tüm tarih alanlarını doldurun');
        return;
    }
    
    try {
        const data = await ApiClient.post('/Reports/GetComparisonData', {
            dataType: dataType,
            period1Start: p1Start,
            period1End: p1End,
            period2Start: p2Start,
            period2End: p2End,
            periodType: periodType,
            customDays: customDays
        });
        
        ComparisonModule.renderTable('comparisonTable', data);
        ComparisonModule.renderChart('comparisonChart', data);
        document.getElementById('comparisonResults').style.display = 'block';
        showSuccess('Karşılaştırma tamamlandı');
    } catch (error) {
        console.error('Comparison error:', error);
    }
}

function exportCsv() {
    ApiClient.download('/Reports/ExportCsv', currentFilters);
}

function exportExcel() {
    ApiClient.download('/Reports/ExportExcel', currentFilters);
}

// Dönem 1 tarihi değişince Dönem 2'yi aynı ay/gün, +1 yıl olarak güncelle
function syncPeriod2Date(sourceId, targetId) {
    const sourceVal = document.getElementById(sourceId).value;
    if (!sourceVal) return;

    const source = new Date(sourceVal);
    if (isNaN(source.getTime())) return;

    const target = new Date(source);
    target.setFullYear(target.getFullYear() + 1);

    // Gelecek tarih olmaması için kontrol
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (target > today) {
        target.setTime(today.getTime());
    }

    // yyyy-MM-dd formatında ata
    document.getElementById(targetId).value = target.toISOString().substring(0, 10);
}






