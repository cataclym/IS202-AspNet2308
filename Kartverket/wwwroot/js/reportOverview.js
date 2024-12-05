// Close the dropdown if the user clicks outside of it
window.onclick = (event) => {
    if (!event.target.matches('.dropbtn') && !event.target.closest('.dropdown-content')) {
        const dropdowns = document.getElementsByClassName("dropdown-content");
        for (const openDropdown of dropdowns) {
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
};

// Sort table by a specific column
const currentSortOrder = { column: -1, order: "asc" };
const selectedFilters = new Set();
const selectedSorts = new Map();

const originalTable = document.querySelector('#admin-table tbody') || document.querySelector('#user-table tbody');
// Lagre originale utgave av tabellen
const savedTable = originalTable.innerHTML;

// Function to show/hide dropdown
function showDropdown() {
    console.log("Dropdown button clicked"); // For debugging
    document.getElementById("myDropdown").classList.toggle("show");
}

function sortTable(columnIndex, element) {
    const table = document.querySelector('#admin-table tbody') || document.querySelector('#user-table tbody');
    if (!table) return;
    
    // Bestem riktig kolonne for dato avhengig av brukertype
    const dateColumnIndex = 4; // 4 for admin, 3 for bruker

    // Toggle sort order
    if (currentSortOrder.column === columnIndex) {
        currentSortOrder.order = currentSortOrder.order === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortOrder.column = columnIndex;
        currentSortOrder.order = 'asc';
    }

    const rows = Array.from(table.rows);
    // Sort rows by pinned status first, then by the selected column
    rows.sort((rowA, rowB) => {
        const isPinnedA = rowA.dataset.isPinned === 'true';
        const isPinnedB = rowB.dataset.isPinned === 'true';

        // Prioritize pinned rows
        if (isPinnedA && !isPinnedB) return -1;
        if (!isPinnedA && isPinnedB) return 1;

        // If both are pinned or both are not pinned, apply column-specific sorting
        let comparison = 0;

        // Hvis vi sorterer etter dato
        if (columnIndex === dateColumnIndex) {
            const dateA = new Date(rowA.cells[dateColumnIndex].dataset.createdAt);
            const dateB = new Date(rowB.cells[dateColumnIndex].dataset.createdAt);

            // Håndter tilfeller med ugyldige datoer
            if (isNaN(dateA) || isNaN(dateB)) {
                console.warn("Ugyldig dato funnet i en rad. Kontrollér datoformatet.");
                return 0;
            }

            // Sammenlign datoene
            comparison = dateA - dateB;  // Sorter etter dato

        } else {
            const cellA = rowA.cells[columnIndex].innerText.trim().toLowerCase();
            const cellB = rowB.cells[columnIndex].innerText.trim().toLowerCase();
            comparison = cellA.localeCompare(cellB);
        }

        return currentSortOrder.order === 'asc' ? comparison : -comparison;
    });

    // Clear the table and append sorted rows
    table.innerHTML = '';
    rows.forEach(row => table.appendChild(row));

    clearActiveFilters();
    if (element) element.classList.add('active-filter');
}

function filterTable() {
    const table = document.querySelector('#admin-table tbody') || document.querySelector('#user-table tbody');
    if (!table) return;

    const rows = table.querySelectorAll('tr');
    rows.forEach(row => {
        const rowStatus = row.dataset.status;
        const rowDate = row.getAttribute('data-date'); // Ensure this matches the data attribute format
        const rowUsername = row.querySelector('td:nth-child(3)') ? row.querySelector('td:nth-child(3)').innerText.trim().toLowerCase() : "";
        const isPinned = row.dataset.isPinned === 'true';

        // Update the matches conditions to include the newly added filters
        const matchesStatus = !selectedFilters.size || selectedFilters.has(rowStatus);
        const matchesDate = selectedSorts.has('dato') ? (selectedSorts.get('dato') === rowDate) : true; // Ensure correct comparison
        const matchesUsername = selectedSorts.has('brukernavn') ? (selectedSorts.get('brukernavn') === rowUsername) : true;

        // Show the row if it is pinned or matches all active filters
        row.style.display = isPinned || (matchesStatus && matchesDate && matchesUsername) ? '' : 'none';
    });
}


function resetFilters() {
    // Fjern alle elementer fra valgt sett
    selectedFilters.clear();
    selectedSorts.clear();
    
    // Skjul alle checkmark-ikoner
    const checkmarkIcons = document.querySelectorAll('.checkmark-icon');
    checkmarkIcons.forEach(icon => {
        icon.style.display = 'none';
    });

    // Clear active filter class from dropdown items
    clearActiveFilters();

    // Oppdater tabellen for å vise alle rader
    filterTable();

    // Reset table sorting (fjern sortering)
    resetTableSorting();
}

// Tilbakestilling av tabellens sortering
function resetTableSorting() {
    // Tilbakestiller sorterings rekkefølge
    currentSortOrder.column = -1;
    currentSortOrder.order = "asc";

    // Tilbakestiller tabellen ved å hente elementet i dokumentet
    let tableBody = document.querySelector('#admin-table tbody') || document.querySelector('#user-table tbody');
    if (tableBody) {
        // Deretter skriver over HTML-en i tabellen med den originale versjonen
        tableBody.innerHTML = savedTable;
    }
}

// Helper function to remove 'active-filter' class from all filter options
function clearActiveFilters() {
    const options = document.querySelectorAll('.dropdown-content a');
    options.forEach(option => option.classList.remove('active-filter'));
}

// Universell toggle-funksjon som håndterer både status og sortering (dato og brukernavn)
function toggleFilter(filterType, element) {
    const isSelected = selectedFilters.has(filterType);

    // Legg til eller fjern filterType fra settet
    if (isSelected) {
        selectedFilters.delete(filterType);
    } else {
        selectedFilters.add(filterType);
    }

    // Oppdater visningen av checkmark-ikonet
    const checkmarkIcon = element.querySelector('.checkmark-icon');
    if (checkmarkIcon) {
        checkmarkIcon.style.display = isSelected ? 'none' : 'inline';
    }

    // Filtrer tabellen basert på aktive valg
    filterTable();
}

// Helper function to handle pin/unpin AJAX requests
function togglePin(reportId, actionUrl, button) {
    const token = $('#reportForm').find('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: actionUrl,
        type: 'POST',
        data: {
            __RequestVerificationToken: token,
            reportId: reportId
        },
        success: (response) => {
            if (response.success) {
                // Toggle button classes
                button.toggleClass('btn-pinned btn-pin');
                // Update button text
                button.html(response.isPinned
                    ? '<i class="bi bi-pin-fill"></i>'  // Unpin ikon
                    : '<i class="bi bi-pin"></i>');      // Pin ikon
                // Update data attribute
                button.attr('data-is-pinned', response.isPinned.toString().toLowerCase());
                // Move the row based on the new pin state
                const row = button.closest('tr');

                row.attr('data-is-pinned', response.isPinned.toString().toLowerCase());

                sortTableByPinnedStatus();
            } else {
                console.warn("Action failed:", response.message);
                // Optionally, notify the user about the failure
            }
        }
    });
}

// Function to sort the table by pinned status and date
function sortTableByPinnedStatus() {
    const table = document.querySelector('#admin-table tbody') || document.querySelector('#user-table tbody');
    if (!table) return;

    const rows = Array.from(table.rows);

    // Sort rows by pinned status first, then by created date (latest first)
    rows.sort((rowA, rowB) => {
        const isPinnedA = rowA.dataset.isPinned === 'true';
        const isPinnedB = rowB.dataset.isPinned === 'true';

        // Prioritize pinned rows
        if (isPinnedA && !isPinnedB) return -1;
        if (!isPinnedA && isPinnedB) return 1;

        // Sort by creation date (newest first)
        const dateA = new Date(rowA.querySelector('td[data-created-at]').getAttribute('data-created-at'));
        const dateB = new Date(rowB.querySelector('td[data-created-at]').getAttribute('data-created-at'));
        return dateB - dateA;  // Newest first
    });

    // Clear the table and append sorted rows
    table.innerHTML = '';
    rows.forEach(row => table.appendChild(row));
}

$(document).ready(() => {
    // Use event delegation to handle dynamically sorted/reordered buttons
    $(document).on('click', '.pin-button', function (e) {
        e.preventDefault();

        const button = $(this);
        const reportId = button.attr('data-report-id'); // Use .attr() instead of .data()
        const isPinned = button.attr('data-is-pinned') === 'true'; // Use .attr() instead of .data()

        console.log("Attempting to " + (isPinned ? "unpin" : "pin") + " Report ID:", reportId); // Debugging line

        // Determine which action to call based on the current state
        const actionUrl = isPinned ? '/Reports/UnpinReport' : '/Reports/PinReport';

        togglePin(reportId, actionUrl, button);
    });

    // Initial sort on page load
    sortTableByPinnedStatus();
});