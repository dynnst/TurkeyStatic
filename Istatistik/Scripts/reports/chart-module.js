// Chart Module - Chart.js entegrasyonu
const ChartModule = (function() {
    let chartInstance = null;
    
    function createChart(canvasId, data, type = 'line') {
        destroyChart();
        const ctx = document.getElementById(canvasId).getContext('2d');
        
        const chartData = {
            labels: data.map(d => d.Label || formatDate(d.Date)),
            datasets: [{
                label: 'Değer',
                data: data.map(d => d.Value),
                backgroundColor: type === 'pie' ? generateColors(data.length) : 'rgba(13, 110, 253, 0.5)',
                borderColor: 'rgba(13, 110, 253, 1)',
                borderWidth: 2,
                fill: type === 'line'
            }]
        };
        
        chartInstance = new Chart(ctx, {
            type: type,
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        display: type === 'pie'
                    },
                    tooltip: {
                        callbacks: {
                            title: (items) => items[0].label,
                            label: (item) => formatNumber(item.parsed.y || item.parsed)
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
        if (!chartInstance) return;
        
        const currentData = chartInstance.data;
        const canvas = chartInstance.canvas;
        const canvasId = canvas.id;
        
        destroyChart();
        
        // Veriyi yeniden oluştur
        const data = currentData.labels.map((label, i) => ({
            Label: label,
            Value: currentData.datasets[0].data[i]
        }));
        
        createChart(canvasId, data, type);
    }
    
    function downloadChart(format) {
        if (!chartInstance) return;
        
        const url = format === 'png' 
            ? chartInstance.toBase64Image('image/png', 1.0)
            : chartInstance.toBase64Image('image/jpeg', 0.9);
        
        const link = document.createElement('a');
        link.download = `rapor-${Date.now()}.${format}`;
        link.href = url;
        link.click();
    }
    
    function generateColors(count) {
        const colors = [
            'rgba(13, 110, 253, 0.7)',
            'rgba(25, 135, 84, 0.7)',
            'rgba(220, 53, 69, 0.7)',
            'rgba(255, 193, 7, 0.7)',
            'rgba(13, 202, 240, 0.7)',
            'rgba(111, 66, 193, 0.7)'
        ];
        
        const result = [];
        for (let i = 0; i < count; i++) {
            result.push(colors[i % colors.length]);
        }
        return result;
    }
    
    return {
        create: createChart,
        destroy: destroyChart,
        changeType: changeChartType,
        download: downloadChart
    };
})();
