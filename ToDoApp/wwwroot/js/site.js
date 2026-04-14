function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
}

document.addEventListener('DOMContentLoaded', function () {
    const getCellValue = (tr, idx) => tr.children[idx].innerText || tr.children[idx].textContent;

    const comparer = (idx, asc) => (a, b) => ((v1, v2) =>
        v1 !== '' && v2 !== '' && !isNaN(v1) && !isNaN(v2) ? v1 - v2 : v1.toString().localeCompare(v2)
    )(getCellValue(asc ? a : b, idx), getCellValue(asc ? b : a, idx));

    document.querySelectorAll('th.sortable').forEach(th => {
        th.addEventListener('click', (() => {
            const table = th.closest('table');
            const tbody = table.querySelector('tbody');

            // Toggle ascending/descending
            th.asc = !th.asc;

            // Add icon feedback
            document.querySelectorAll('th.sortable i').forEach(icon => icon.remove());
            const icon = document.createElement('i');
            icon.className = th.asc ? 'bi bi-arrow-up-short ms-1' : 'bi bi-arrow-down-short ms-1';
            th.appendChild(icon);

            // Sort
            Array.from(tbody.querySelectorAll('tr'))
                .sort(comparer(Array.from(th.parentNode.children).indexOf(th), th.asc))
                .forEach(tr => tbody.appendChild(tr));
        }));
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const body = document.body;

    if (!darkModeToggle) return;

    // Check saved preference
    const currentTheme = localStorage.getItem('theme') || 'light';

    // Apply theme on load
    if (currentTheme === 'dark') {
        body.classList.add('dark-mode');
        darkModeToggle.innerHTML = '<i class="bi bi-sun-fill text-warning"></i>';
    } else {
        darkModeToggle.innerHTML = '<i class="bi bi-moon-stars-fill"></i>';
    }

    // Handle toggle click
    darkModeToggle.addEventListener('click', () => {
        body.classList.toggle('dark-mode');
        let theme = 'light';

        if (body.classList.contains('dark-mode')) {
            theme = 'dark';
            darkModeToggle.innerHTML = '<i class="bi bi-sun-fill text-warning"></i>';
        } else {
            darkModeToggle.innerHTML = '<i class="bi bi-moon-stars-fill"></i>';
        }

        localStorage.setItem('theme', theme); // Save preference
    });
});