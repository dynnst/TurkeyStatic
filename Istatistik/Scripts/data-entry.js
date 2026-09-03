// API Base URL
const API_BASE_URL = '';

// Global State
let currentUnit = null;
let currentDate = new Date().toISOString().split('T')[0];

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    initializePage();
    setupEventListeners();
});

// ============================================================================
// INITIALIZATION
// ============================================================================

function initializePage() {
    // Set today's date as default
    const dateInput = document.getElementById('entryDate');
    dateInput.value = new Date().toISOString().split('T')[0];

    // Load units
    loadUnits();

    // Set current user
    setCurrentUser();
}

function loadUnits() {
    showSpinner(true);
    fetch(`${API_BASE_URL}/Units/GetAll`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.data) {
                populateUnitSelect(data.data);
            }
        })
        .catch(error => {
            showAlert('Birimler yüklenirken hata: ' + error.message, 'danger');
            console.error('Error loading units:', error);
        })
        .finally(() => showSpinner(false));
}

function populateUnitSelect(units) {
    const select = document.getElementById('unitSelect');
    select.innerHTML = '<option value="">Birim Seçiniz...</option>';

    units.forEach(unit => {
        const option = document.createElement('option');
        option.value = unit.UnitId;
        option.textContent = unit.UnitName;
        select.appendChild(option);
    });

    // Select first unit by default
    if (units.length > 0) {
        select.value = units[0].UnitId;
        currentUnit = units[0];
        loadDataForCurrentDate();
    }
}

function setCurrentUser() {
    const username = 'Kullanýcý'; // Ýleride authentication ile alýnacak
    document.getElementById('kullaniciAdi').textContent = `?? ${username}`;
}

// ============================================================================
// EVENT LISTENERS
// ============================================================================

function setupEventListeners() {
    // Unit selection
    document.getElementById('unitSelect').addEventListener('change', function () {
        const unitId = this.value;
        if (unitId) {
            currentUnit = parseInt(unitId);
            loadDataForCurrentDate();
        }
    });

    // Date selection
    document.getElementById('entryDate').addEventListener('change', function () {
        currentDate = this.value;
        loadDataForCurrentDate();
    });

    // Crime Form
    document.getElementById('crimeSaveBtn').addEventListener('click', saveCrimeStatistic);
    document.getElementById('crimeClearBtn').addEventListener('click', () => clearForm('crimeForm'));

    // Query Form
    document.getElementById('querySaveBtn').addEventListener('click', saveQueryStatistic);
    document.getElementById('queryClearBtn').addEventListener('click', () => clearForm('queryForm'));

    // Activity Form
    document.getElementById('activitySaveBtn').addEventListener('click', saveActivityStatistic);
    document.getElementById('activityClearBtn').addEventListener('click', () => clearForm('activityForm'));
}

// ============================================================================
// CRIME STATISTICS
// ============================================================================

function saveCrimeStatistic() {
    const unitId = document.getElementById('unitSelect').value;
    const entryDate = document.getElementById('entryDate').value;

    if (!unitId || !entryDate) {
        showAlert('Lütfen Birim ve Tarih seçiniz!', 'warning');
        return;
    }

    const crimeData = {
        UnitId: parseInt(unitId),
        EntryDate: new Date(entryDate).toISOString(),
        CrimeType: document.getElementById('crimeType').value,
        CrimeCode: document.getElementById('crimeCode').value,
        CasesRequiringFollow_up: parseInt(document.getElementById('casesRequiringFollowUp').value) || 0,
        SuspectCount: parseInt(document.getElementById('crimesSuspectCount').value) || 0,
        ArrestedCount: parseInt(document.getElementById('crimesArrestedCount').value) || 0,
        JudicialControlCount: parseInt(document.getElementById('crimesJudicialControl').value) || 0,
        Notes: document.getElementById('crimesNotes').value,
        CreatedBy: 'System'
    };

    // Validation
    if (!crimeData.CrimeType.trim()) {
        showAlert('Suç Türü boþ olamaz!', 'warning');
        return;
    }

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimeStatistics/Create`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(crimeData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Suç istatistiði baþarýyla kaydedildi!', 'success');
                clearForm('crimeForm');
                loadCrimeStatistics();
                updateSummaryCards();
            } else {
                showAlert(data.message || 'Kayýt hatasý!', 'danger');
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
            console.error('Error saving crime statistic:', error);
        })
        .finally(() => showSpinner(false));
}

function loadCrimeStatistics() {
    const unitId = document.getElementById('unitSelect').value;
    const date = document.getElementById('entryDate').value;

    if (!unitId || !date) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimeStatistics/GetByDate?unitId=${unitId}&date=${date}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.data) {
                populateCrimeTable(data.data);
            }
        })
        .catch(error => {
            console.error('Error loading crime statistics:', error);
        })
        .finally(() => showSpinner(false));
}

function populateCrimeTable(crimes) {
    const tbody = document.getElementById('crimeTableBody');
    tbody.innerHTML = '';

    crimes.forEach(crime => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${crime.CrimeType}</td>
            <td><span class="badge bg-info">${crime.CrimeCode || '-'}</span></td>
            <td>${crime.SuspectCount}</td>
            <td>${crime.ArrestedCount}</td>
            <td>${new Date(crime.EntryDate).toLocaleDateString('tr-TR')}</td>
            <td>
                <button class="btn btn-sm btn-warning" onclick="editCrimeStatistic(${crime.CrimeStatisticId})">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteCrimeStatistic(${crime.CrimeStatisticId})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });

    if (crimes.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Veri bulunamadý</td></tr>';
    }
}

function deleteCrimeStatistic(id) {
    if (!confirm('Silmek istediðinizden emin misiniz?')) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimeStatistics/Delete`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id: id })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Kayýt baþarýyla silindi!', 'success');
                loadCrimeStatistics();
                updateSummaryCards();
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
        })
        .finally(() => showSpinner(false));
}

// ============================================================================
// QUERY STATISTICS
// ============================================================================

function saveQueryStatistic() {
    const unitId = document.getElementById('unitSelect').value;
    const entryDate = document.getElementById('entryDate').value;

    if (!unitId || !entryDate) {
        showAlert('Lütfen Birim ve Tarih seçiniz!', 'warning');
        return;
    }

    const queryData = {
        UnitId: parseInt(unitId),
        EntryDate: new Date(entryDate).toISOString(),
        Shift: document.getElementById('queryShift').value,
        OperationType: document.getElementById('queryOperationType').value,
        PersonQueriedCount: parseInt(document.getElementById('queryPersonQueried').value) || 0,
        PersonArrestedSearchedCount: parseInt(document.getElementById('queryPersonArrestedSearched').value) || 0,
        Notes: document.getElementById('queryNotes').value,
        CreatedBy: 'System'
    };

    showSpinner(true);
    fetch(`${API_BASE_URL}/QueryStatistics/Create`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(queryData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Sorgu istatistiði baþarýyla kaydedildi!', 'success');
                clearForm('queryForm');
                loadQueryStatistics();
                updateSummaryCards();
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
        })
        .finally(() => showSpinner(false));
}

function loadQueryStatistics() {
    const unitId = document.getElementById('unitSelect').value;
    const date = document.getElementById('entryDate').value;

    if (!unitId || !date) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/QueryStatistics/GetByDate?unitId=${unitId}&date=${date}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.data) {
                populateQueryTable(data.data);
            }
        })
        .catch(error => {
            console.error('Error loading query statistics:', error);
        })
        .finally(() => showSpinner(false));
}

function populateQueryTable(queries) {
    const tbody = document.getElementById('queryTableBody');
    tbody.innerHTML = '';

    queries.forEach(query => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${query.Shift}</td>
            <td>${query.PersonQueriedCount}</td>
            <td>${query.PersonArrestedSearchedCount}</td>
            <td><span class="badge bg-secondary">${query.OperationType}</span></td>
            <td>${new Date(query.EntryDate).toLocaleDateString('tr-TR')}</td>
            <td>
                <button class="btn btn-sm btn-warning" onclick="editQueryStatistic(${query.QueryStatisticId})">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteQueryStatistic(${query.QueryStatisticId})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });

    if (queries.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Veri bulunamadý</td></tr>';
    }
}

function deleteQueryStatistic(id) {
    if (!confirm('Silmek istediðinizden emin misiniz?')) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/QueryStatistics/Delete`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id: id })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Kayýt baþarýyla silindi!', 'success');
                loadQueryStatistics();
                updateSummaryCards();
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
        })
        .finally(() => showSpinner(false));
}

// ============================================================================
// ACTIVITY STATISTICS
// ============================================================================

function saveActivityStatistic() {
    const unitId = document.getElementById('unitSelect').value;
    const entryDate = document.getElementById('entryDate').value;

    if (!unitId || !entryDate) {
        showAlert('Lütfen Birim ve Tarih seçiniz!', 'warning');
        return;
    }

    const activityData = {
        UnitId: parseInt(unitId),
        EntryDate: new Date(entryDate).toISOString(),
        WarrantSource: document.getElementById('activityWarrantSource').value,
        WarrantCount: parseInt(document.getElementById('activityWarrantCount').value) || 0,
        ApprehendedCount: parseInt(document.getElementById('activityApprehendedCount').value) || 0,
        ArrestedCount: parseInt(document.getElementById('activityArrestedCount').value) || 0,
        ApprehensionLocation: document.getElementById('activityLocation').value,
        Notes: document.getElementById('activityNotes').value,
        CreatedBy: 'System'
    };

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimePreventionActivities/Create`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(activityData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Arama/Yakalama faaliyeti baþarýyla kaydedildi!', 'success');
                clearForm('activityForm');
                loadActivityStatistics();
                updateSummaryCards();
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
        })
        .finally(() => showSpinner(false));
}

function loadActivityStatistics() {
    const unitId = document.getElementById('unitSelect').value;
    const date = document.getElementById('entryDate').value;

    if (!unitId || !date) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimePreventionActivities/GetByDate?unitId=${unitId}&date=${date}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.data) {
                populateActivityTable(data.data);
            }
        })
        .catch(error => {
            console.error('Error loading activity statistics:', error);
        })
        .finally(() => showSpinner(false));
}

function populateActivityTable(activities) {
    const tbody = document.getElementById('activityTableBody');
    tbody.innerHTML = '';

    activities.forEach(activity => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td><span class="badge bg-primary">${activity.WarrantSource}</span></td>
            <td>${activity.ApprehendedCount}</td>
            <td>${activity.ArrestedCount}</td>
            <td>${activity.ApprehensionLocation || '-'}</td>
            <td>${new Date(activity.EntryDate).toLocaleDateString('tr-TR')}</td>
            <td>
                <button class="btn btn-sm btn-warning" onclick="editActivityStatistic(${activity.ActivityId})">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteActivityStatistic(${activity.ActivityId})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });

    if (activities.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Veri bulunamadý</td></tr>';
    }
}

function deleteActivityStatistic(id) {
    if (!confirm('Silmek istediðinizden emin misiniz?')) return;

    showSpinner(true);
    fetch(`${API_BASE_URL}/CrimePreventionActivities/Delete`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id: id })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlert('Kayýt baþarýyla silindi!', 'success');
                loadActivityStatistics();
                updateSummaryCards();
            }
        })
        .catch(error => {
            showAlert('Hata: ' + error.message, 'danger');
        })
        .finally(() => showSpinner(false));
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

function loadDataForCurrentDate() {
    loadCrimeStatistics();
    loadQueryStatistics();
    loadActivityStatistics();
    updateSummaryCards();
}

function updateSummaryCards() {
    const unitId = document.getElementById('unitSelect').value;
    const date = document.getElementById('entryDate').value;

    if (!unitId || !date) return;

    // Load all data and calculate totals
    Promise.all([
        fetch(`${API_BASE_URL}/CrimeStatistics/GetByDate?unitId=${unitId}&date=${date}`).then(r => r.json()),
        fetch(`${API_BASE_URL}/QueryStatistics/GetByDate?unitId=${unitId}&date=${date}`).then(r => r.json()),
        fetch(`${API_BASE_URL}/CrimePreventionActivities/GetByDate?unitId=${unitId}&date=${date}`).then(r => r.json())
    ])
        .then(([crimes, queries, activities]) => {
            let totalCrimes = 0;
            let totalSuspects = 0;
            let totalArrested = 0;
            let totalQueries = 0;

            if (crimes.data) {
                totalCrimes = crimes.data.length;
                totalSuspects = crimes.data.reduce((sum, c) => sum + (c.SuspectCount || 0), 0);
                totalArrested = crimes.data.reduce((sum, c) => sum + (c.ArrestedCount || 0), 0);
            }

            if (queries.data) {
                totalQueries = queries.data.reduce((sum, q) => sum + (q.PersonQueriedCount || 0), 0);
            }

            document.getElementById('totalCrimes').textContent = totalCrimes;
            document.getElementById('totalSuspects').textContent = totalSuspects;
            document.getElementById('totalArrested').textContent = totalArrested;
            document.getElementById('totalQueries').textContent = totalQueries;
        })
        .catch(error => console.error('Error updating summary:', error));
}

function clearForm(formId) {
    document.getElementById(formId).reset();
}

function showAlert(message, type = 'info') {
    const alertContainer = document.getElementById('alertContainer');
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    alertContainer.appendChild(alert);

    // Auto dismiss after 5 seconds
    setTimeout(() => {
        alert.remove();
    }, 5000);
}

function showSpinner(show) {
    const spinner = document.getElementById('loadingSpinner');
    if (show) {
        spinner.classList.add('active');
    } else {
        spinner.classList.remove('active');
    }
}
