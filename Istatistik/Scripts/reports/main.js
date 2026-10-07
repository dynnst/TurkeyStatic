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

    document.getElementById('startDate').valueAsDate = oneMonthAgo;
    document.getElementById('endDate').valueAsDate = today;

    // Dönem 1 başlangıç: bir yıl önce, aynı ay/gün
    const p1Start = new Date(oneMonthAgo);
    p1Start.setFullYear(p1Start.getFullYear() - 1);
    const p1End = new Date(today);
    p1End.setFullYear(p1End.getFullYear() - 1);

    document.getElementById('p1StartDate').valueAsDate = p1Start;
    document.getElementById('p1EndDate').valueAsDate   = p1End;

    // Dönem 2: aynı ay/gün, bu yıl (bir yıl sonrası)
    document.getElementById('p2StartDate').valueAsDate = oneMonthAgo;
    document.getElementById('p2EndDate').valueAsDate   = today;

    // Dönem 1 tarih değişince → Dönem 2'yi otomatik güncelle (aynı ay/gün, +1 yıl)
    document.getElementById('p1StartDate').addEventListener('change', function() {
        syncPeriod2Date('p1StartDate', 'p2StartDate');
    });
    document.getElementById('p1EndDate').addEventListener('change', function() {
        syncPeriod2Date('p1EndDate', 'p2EndDate');
    });
    
    // Dönem tipi değişikliğinde özel gün sayısını göster/gizle
    document.getElementById('periodType').addEventListener('change', function() {
        const customContainer = document.getElementById('customPeriodContainer');
        const btnContainer = document.getElementById('btnFilterContainer');
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
    document.getElementById('periodType').dispatchEvent(new Event('change'));
    
    // Event listeners
    document.getElementById('btnFilter').addEventListener('click', loadChartData);
    document.getElementById('chartType').addEventListener('change', function() {
        ChartModule.changeType(this.value);
    });
    document.getElementById('btnDownloadPng').addEventListener('click', () => ChartModule.download('png'));
    document.getElementById('btnDownloadJpeg').addEventListener('click', () => ChartModule.download('jpeg'));
    document.getElementById('pageSize').addEventListener('change', function() {
        TableModule.setPageSize(parseInt(this.value));
        loadTableData(1);
    });
    document.getElementById('btnExportCsv').addEventListener('click', exportCsv);
    document.getElementById('btnExportExcel').addEventListener('click', exportExcel);
    document.getElementById('btnCompare').addEventListener('click', loadComparisonData);
    
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
