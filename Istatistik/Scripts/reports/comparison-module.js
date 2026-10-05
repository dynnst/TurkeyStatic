// Comparison Module - Dönem karşılaştırma
const ComparisonModule = (function() {
    let comparisonChartInstance = null;

    // ── Tarih Yardımcıları ──────────────────────────────────────────────────

    // ASP.NET JSON serializer /Date(ms)/ veya ISO yyyy-MM-dd → dd.MM.yyyy
    function formatDateTR(val) {
        if (!val) return '';
        var msMatch = String(val).match(/\/Date\((-?\d+)\)\//);
        if (msMatch) {
            var d = new Date(parseInt(msMatch[1]));
            return pad(d.getDate()) + '.' + pad(d.getMonth() + 1) + '.' + d.getFullYear();
        }
        var s = String(val).substring(0, 10);
        var parts = s.split('-');
        if (parts.length === 3) return parts[2] + '.' + parts[1] + '.' + parts[0];
        return val;
    }

    function pad(n) { return n < 10 ? '0' + n : String(n); }

    function formatDateRange(start, end) {
        return formatDateTR(start) + ' – ' + formatDateTR(end);
    }

    // ── Tablo ───────────────────────────────────────────────────────────────

    function renderComparisonTable(containerId, comparisonData) {
        var container = document.getElementById(containerId);

        if (!comparisonData.Differences || comparisonData.Differences.length === 0) {
            container.innerHTML = '<div class="alert alert-info">Karşılaştırma verisi bulunamadı</div>';
            return;
        }

        var p1Start = formatDateTR(comparisonData.Period1Start);
        var p1End   = formatDateTR(comparisonData.Period1End);
        var p2Start = formatDateTR(comparisonData.Period2Start);
        var p2End   = formatDateTR(comparisonData.Period2End);

        // Dönem özet kartları
        var html = '<div class="row mb-3">';

        html += '<div class="col-md-5">';
        html +=   '<div class="card border-primary h-100">';
        html +=     '<div class="card-header bg-primary text-white py-2">';
        html +=       '<i class="bi bi-calendar-range"></i> <strong>Dönem 1</strong>';
        html +=     '</div>';
        html +=     '<div class="card-body py-2 text-center">';
        html +=       '<span class="fw-semibold">' + p1Start + '</span>';
        html +=       ' <span class="text-muted">→</span> ';
        html +=       '<span class="fw-semibold">' + p1End + '</span>';
        html +=     '</div>';
        html +=   '</div>';
        html += '</div>';

        html += '<div class="col-md-2 d-flex align-items-center justify-content-center">';
        html +=   '<span class="display-6 text-muted">⇄</span>';
        html += '</div>';

        html += '<div class="col-md-5">';
        html +=   '<div class="card border-success h-100">';
        html +=     '<div class="card-header bg-success text-white py-2">';
        html +=       '<i class="bi bi-calendar-range"></i> <strong>Dönem 2</strong>';
        html +=     '</div>';
        html +=     '<div class="card-body py-2 text-center">';
        html +=       '<span class="fw-semibold">' + p2Start + '</span>';
        html +=       ' <span class="text-muted">→</span> ';
        html +=       '<span class="fw-semibold">' + p2End + '</span>';
        html +=     '</div>';
        html +=   '</div>';
        html += '</div>';

        html += '</div>';

        // Karşılaştırma tablosu
        html += '<div class="table-responsive">';
        html += '<table class="table table-bordered table-hover align-middle">';
        html += '<thead class="table-dark">';
        html += '<tr>';
        html += '<th rowspan="2" class="align-middle">Metrik</th>';
        html += '<th class="text-center" style="color:#90caf9;">① Dönem 1</th>';
        html += '<th class="text-center" style="color:#a5d6a7;">② Dönem 2</th>';
        html += '<th rowspan="2" class="text-center align-middle">Mutlak Fark</th>';
        html += '<th rowspan="2" class="text-center align-middle">Değişim %</th>';
        html += '</tr>';
        html += '<tr>';
        html += '<th class="text-center fw-normal small">' + p1Start + ' – ' + p1End + '</th>';
        html += '<th class="text-center fw-normal small">' + p2Start + ' – ' + p2End + '</th>';
        html += '</tr>';
        html += '</thead><tbody>';

        comparisonData.Differences.forEach(function(row) {
            var trendIcon  = getTrendIcon(row.Trend);
            var trendClass = getTrendClass(row.Trend);
            var trendBg    = row.Trend === 0 ? 'table-success'
                           : row.Trend === 1 ? 'table-danger'
                           : '';

            var percentText;
            if (row.PercentageChange === null || row.PercentageChange === undefined) {
                percentText = '∞';
            } else {
                percentText = parseFloat(row.PercentageChange).toFixed(2);
            }

            var diffPrefix = row.AbsoluteDifference > 0 ? '+' : '';

            html += '<tr class="' + trendBg + '">';
            html += '<td><strong>' + row.MetricName + '</strong></td>';
            html += '<td class="text-center">' + formatNumber(row.Period1Value) + '</td>';
            html += '<td class="text-center">' + formatNumber(row.Period2Value) + '</td>';
            html += '<td class="text-center ' + trendClass + ' fw-bold">' +
                      diffPrefix + formatNumber(row.AbsoluteDifference) +
                    '</td>';
            html += '<td class="text-center ' + trendClass + '">' +
                      '<span class="fw-bold fs-6">' + trendIcon + '</span> ' +
                      '<span class="fw-bold">' + percentText + '%</span>' +
                    '</td>';
            html += '</tr>';
        });

        html += '</tbody></table>';
        html += '</div>';

        container.innerHTML = html;
    }

    // ── Grafik ──────────────────────────────────────────────────────────────

    function renderComparisonChart(canvasId, comparisonData) {
        if (comparisonChartInstance) {
            comparisonChartInstance.destroy();
            comparisonChartInstance = null;
        }

        var canvas = document.getElementById(canvasId);
        if (!canvas) return;

        var p1Label = formatDateRange(comparisonData.Period1Start, comparisonData.Period1End);
        var p2Label = formatDateRange(comparisonData.Period2Start, comparisonData.Period2End);

        var maxLen = Math.max(
            comparisonData.Period1Data.length,
            comparisonData.Period2Data.length
        );
        var labels = [];
        for (var i = 0; i < maxLen; i++) {
            if (i < comparisonData.Period1Data.length) {
                labels.push(comparisonData.Period1Data[i].Label ||
                            formatDateTR(comparisonData.Period1Data[i].Date));
            } else {
                labels.push(comparisonData.Period2Data[i].Label ||
                            formatDateTR(comparisonData.Period2Data[i].Date));
            }
        }

        var ctx = canvas.getContext('2d');
        comparisonChartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: p1Label,
                        data: comparisonData.Period1Data.map(function(d) { return d.Value; }),
                        backgroundColor: 'rgba(13, 110, 253, 0.6)',
                        borderColor: 'rgba(13, 110, 253, 1)',
                        borderWidth: 1
                    },
                    {
                        label: p2Label,
                        data: comparisonData.Period2Data.map(function(d) { return d.Value; }),
                        backgroundColor: 'rgba(25, 135, 84, 0.6)',
                        borderColor: 'rgba(25, 135, 84, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top' },
                    tooltip: {
                        callbacks: {
                            label: function(item) {
                                return item.dataset.label + ': ' + formatNumber(item.parsed.y);
                            }
                        }
                    }
                },
                scales: { y: { beginAtZero: true } }
            }
        });
    }

    // ── Trend Yardımcıları ──────────────────────────────────────────────────

    function getTrendIcon(trend) {
        if (trend === 0) return '↑';
        if (trend === 1) return '↓';
        return '→';
    }

    function getTrendClass(trend) {
        if (trend === 0) return 'text-success';
        if (trend === 1) return 'text-danger';
        return 'text-secondary';
    }

    return {
        renderTable: renderComparisonTable,
        renderChart: renderComparisonChart
    };
})();
