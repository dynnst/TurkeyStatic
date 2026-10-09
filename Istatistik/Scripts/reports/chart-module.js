// Chart Module - Chart.js entegrasyonu
const ChartModule = (function() {
    let chartInstance = null;
    let cachedData = null;
    let cachedCanvasId = 'reportChart';
    let currentChartType = 'line';

    const colorPalette = [
        { bg: 'rgba(13, 110, 253, 0.6)', border: 'rgba(13, 110, 253, 1)' },   // Primary Blue
        { bg: 'rgba(25, 135, 84, 0.6)', border: 'rgba(25, 135, 84, 1)' },     // Success Green
        { bg: 'rgba(220, 53, 69, 0.6)', border: 'rgba(220, 53, 69, 1)' },     // Danger Red
        { bg: 'rgba(255, 193, 7, 0.6)', border: 'rgba(255, 193, 7, 1)' },     // Warning Yellow
        { bg: 'rgba(13, 202, 240, 0.6)', border: 'rgba(13, 202, 240, 1)' },   // Info Cyan
        { bg: 'rgba(111, 66, 193, 0.6)', border: 'rgba(111, 66, 193, 1)' },   // Indigo
        { bg: 'rgba(253, 126, 20, 0.6)', border: 'rgba(253, 126, 20, 1)' },   // Orange
        { bg: 'rgba(32, 201, 151, 0.6)', border: 'rgba(32, 201, 151, 1)' }    // Teal
    ];

    function createChart(canvasId, data, type = 'line') {
        destroyChart();
        cachedCanvasId = canvasId;
        cachedData = data;
        currentChartType = type;

        const canvas = document.getElementById(canvasId);
        if (!canvas) return null;
        const ctx = canvas.getContext('2d');

        if (!data || data.length === 0) return null;

        const labels = data.map(d => d.Label || formatDate(d.Date));
        const first = data[0];
        const hasMetrics = first.Metrics && Object.keys(first.Metrics).length > 0;
        let datasets = [];

        if (type === 'pie') {
            if (hasMetrics) {
                // Pasta grafik: Metriklerin toplam dağılımını göster
                const metricKeys = Object.keys(first.Metrics);
                const metricTotals = metricKeys.map(k => {
                    return data.reduce((sum, d) => sum + ((d.Metrics && d.Metrics[k]) || 0), 0);
                });
                datasets = [{
                    label: 'Dağılım',
                    data: metricTotals,
                    backgroundColor: generateColors(metricKeys.length),
                    borderColor: '#ffffff',
                    borderWidth: 2
                }];
                // Pasta grafik etiketleri metrik isimleri olsun
                chartInstance = new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: metricKeys,
                        datasets: datasets
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { position: 'top' },
                            tooltip: {
                                callbacks: {
                                    label: (item) => `${item.label}: ${formatNumber(item.parsed)}`
                                }
                            }
                        }
                    }
                });
                return chartInstance;
            } else {
                // Zaman dilimlerine göre pasta grafik
                datasets = [{
                    label: 'Değer',
                    data: data.map(d => d.Value),
                    backgroundColor: generateColors(data.length),
                    borderColor: '#ffffff',
                    borderWidth: 1
                }];
            }
        } else {
            // Çizgi veya Sütun grafik
            if (hasMetrics) {
                const metricKeys = Object.keys(first.Metrics);
                metricKeys.forEach((key, idx) => {
                    const c = colorPalette[idx % colorPalette.length];
                    datasets.push({
                        label: key,
                        data: data.map(d => (d.Metrics && d.Metrics[key] !== undefined) ? d.Metrics[key] : 0),
                        backgroundColor: c.bg,
                        borderColor: c.border,
                        borderWidth: 2,
                        fill: type === 'line' ? false : true,
                        tension: 0.1
                    });
                });
            } else {
                const c = colorPalette[0];
                datasets = [{
                    label: 'Değer',
                    data: data.map(d => d.Value),
                    backgroundColor: c.bg,
                    borderColor: c.border,
                    borderWidth: 2,
                    fill: type === 'line' ? false : true,
                    tension: 0.1
                }];
            }
        }

        chartInstance = new Chart(ctx, {
            type: type,
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        display: true
                    },
                    tooltip: {
                        callbacks: {
                            title: (items) => items[0].label,
                            label: (item) => `${item.dataset.label}: ${formatNumber(item.parsed.y !== undefined ? item.parsed.y : item.parsed)}`
                        }
                    }
                },
                scales: type !== 'pie' ? {
                    y: {
                        beginAtZero: true
                    }
                } : {}
            }
        });

        return chartInstance;
    }

    function destroyChart() {
        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }
    }

    function changeChartType(type) {
        currentChartType = type;
        if (cachedData && cachedCanvasId) {
            createChart(cachedCanvasId, cachedData, type);
        }
    }

    function getBase64Image(format = 'png') {
        if (!chartInstance) return '';
        const canvas = chartInstance.canvas;
        if (!canvas) return '';

        if (format === 'jpeg' || format === 'jpg') {
            // JPEG için beyaz arka planlı kopya oluştur
            const tempCanvas = document.createElement('canvas');
            tempCanvas.width = canvas.width;
            tempCanvas.height = canvas.height;
            const tempCtx = tempCanvas.getContext('2d');
            tempCtx.fillStyle = '#ffffff';
            tempCtx.fillRect(0, 0, tempCanvas.width, tempCanvas.height);
            tempCtx.drawImage(canvas, 0, 0);
            return tempCanvas.toDataURL('image/jpeg', 0.95);
        }

        return chartInstance.toBase64Image('image/png', 1.0);
    }

    function downloadChart(format = 'png') {
        if (!chartInstance) return;
        const dataUrl = getBase64Image(format);
        if (!dataUrl) return;

        const ext = format === 'jpeg' ? 'jpg' : 'png';
        const link = document.createElement('a');
        link.download = `rapor-grafik-${Date.now()}.${ext}`;
        link.href = dataUrl;
        link.click();
    }

    function generateColors(count) {
        const result = [];
        for (let i = 0; i < count; i++) {
            result.push(colorPalette[i % colorPalette.length].bg);
        }
        return result;
    }

    return {
        create: createChart,
        destroy: destroyChart,
        changeType: changeChartType,
        download: downloadChart,
        getBase64Image: getBase64Image,
        getInstance: () => chartInstance
    };
})();
