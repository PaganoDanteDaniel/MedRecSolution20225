// wwwroot/js/syncTableWidths.js

// Función para sincronizar anchos
window.syncTableWidths = function () {
    const headerTable = document.getElementById('header-table');
    const bodyTable = document.getElementById('body-table');

    if (!headerTable || !bodyTable) return;

    const headerCells = Array.from(headerTable.querySelectorAll('th'));
    const bodyRows = bodyTable.querySelectorAll('tr');

    if (bodyRows.length === 0) return;

    const firstBodyRow = bodyRows[0];
    const bodyCells = Array.from(firstBodyRow.querySelectorAll('td'));

    if (headerCells.length !== bodyCells.length) return;

    // Ignorar la primera columna (índice 0)
    for (let i = 1; i < headerCells.length; i++) {
        const bodyCellWidth = bodyCells[i].offsetWidth;
        const headerCellWidth = headerCells[i].offsetWidth;
        const finalWidth = Math.max(bodyCellWidth, headerCellWidth);

        headerCells[i].style.width = `${finalWidth}px`;
        bodyCells[i].style.width = `${finalWidth}px`;
    }
};

// Ejecutar al cargar y al redimensionar
window.syncTableWidths(); // opcional: por si se carga antes
window.addEventListener('load', window.syncTableWidths);
window.addEventListener('resize', window.syncTableWidths);