// Table Module - Tablo yönetimi ve sıralama
const TableModule = (function() {
    let currentData = [];
    let currentSort = { column: null, desc: false };
    let currentPage = 1;
    let pageSize = 25;
    
    function renderTable(containerId, pagedResult) {
        const container = document.getElementById(containerId);
        
        if (!pagedResult.Rows || pagedResult.Rows.length === 0) {
            container.innerHTML = '<div class="alert alert-info">Gösterilecek veri yok</div>';
            return;
        }
        
        currentData = pagedResult;
        
        let html = '<table class="table table-striped table-hover">';
        html += '<thead class="table-light"><tr>';
        
        const firstRow = pagedResult.Rows[0];
        for (const key in firstRow) {
            const sortIcon = getSortIcon(key);
            html += `<th style="cursor:pointer" data-sort="${key}">${key} ${sortIcon}</th>`;
        }
        html += '</tr></thead><tbody>';
        
        pagedResult.Rows.forEach(row => {
            html += '<tr>';
            for (const key in row) {
                html += `<td>${row[key]}</td>`;
            }
            html += '</tr>';
        });
        
        html += '</tbody>';
        
        if (pagedResult.Summary) {
            html += '<tfoot class="table-group-divider fw-bold bg-light"><tr>';
            for (const key in firstRow) {
                let sumKey = key + ' (Toplam)';
                if (pagedResult.Summary[sumKey] !== undefined) {
                    html += `<td>${formatNumber(pagedResult.Summary[sumKey])}</td>`;
                } else if (pagedResult.Summary["Toplam"] !== undefined && key === "Değer") {
                    html += `<td>${formatNumber(pagedResult.Summary["Toplam"])}</td>`;
                } else if (key === 'Tarih') {
                    html += `<td>GENEL TOPLAM</td>`;
                } else {
                    html += `<td></td>`;
                }
            }
            html += '</tr></tfoot>';
        }
        
        html += '</table>';
        container.innerHTML = html;
        
        attachSortHandlers(containerId);
        renderPagination(pagedResult);
        renderSummary(pagedResult.Summary);
    }
    
    function renderPagination(pagedResult) {
        const container = document.getElementById('tablePagination');
        if (pagedResult.TotalPages <= 1) {
            container.innerHTML = '';
            return;
        }
        
        let html = '<ul class="pagination justify-content-center">';
        
        // Previous
        html += `<li class="page-item ${pagedResult.CurrentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${pagedResult.CurrentPage - 1}">Önceki</a>
        </li>`;
        
        // Pages
        for (let i = 1; i <= pagedResult.TotalPages; i++) {
            if (i === 1 || i === pagedResult.TotalPages || Math.abs(i - pagedResult.CurrentPage) <= 2) {
                html += `<li class="page-item ${i === pagedResult.CurrentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>`;
            } else if (Math.abs(i - pagedResult.CurrentPage) === 3) {
                html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
        }
        
        // Next
        html += `<li class="page-item ${pagedResult.CurrentPage === pagedResult.TotalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${pagedResult.CurrentPage + 1}">Sonraki</a>
        </li>`;
        
        html += '</ul>';
        container.innerHTML = html;
        
        // Pagination click handlers
        container.querySelectorAll('a.page-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = parseInt(link.dataset.page);
                if (page && page !== pagedResult.CurrentPage) {
                    currentPage = page;
                    window.loadTableData(page);
                }
            });
        });
    }
    
    function renderSummary(summary) {
        const container = document.getElementById('tableSummary');
        if (!summary) {
            container.innerHTML = '';
            return;
        }
        
        let html = '<div class="d-flex flex-wrap gap-3 mt-2">';
        for (const key in summary) {
            if (key.includes('(Toplam)') || key === 'Toplam') continue; // Toplamlar artık tablonun alt satırında
            html += `<span class="badge bg-secondary px-3 py-2 fs-6 fw-normal">${key}: <strong class="fw-bold">${formatNumber(summary[key])}</strong></span>`;
        }
        html += '</div>';
        container.innerHTML = html;
    }
    
    function attachSortHandlers(containerId) {
        const headers = document.querySelectorAll(`#${containerId} th[data-sort]`);
        headers.forEach(header => {
            header.addEventListener('click', () => {
                const column = header.dataset.sort;
                const desc = currentSort.column === column ? !currentSort.desc : false;
                currentSort = { column, desc };
                window.loadTableData(1, column, desc);
            });
        });
    }
    
    function getSortIcon(column) {
        if (currentSort.column !== column) return '<i class="bi bi-arrow-down-up text-muted"></i>';
        return currentSort.desc ? '<i class="bi bi-arrow-down"></i>' : '<i class="bi bi-arrow-up"></i>';
    }
    
    function setPageSize(size) {
        pageSize = size;
        currentPage = 1;
    }
    
    function getPageSize() {
        return pageSize;
    }
    
    function getCurrentPage() {
        return currentPage;
    }
    
    return {
        render: renderTable,
        setPageSize: setPageSize,
        getPageSize: getPageSize,
        getCurrentPage: getCurrentPage
    };
})();
