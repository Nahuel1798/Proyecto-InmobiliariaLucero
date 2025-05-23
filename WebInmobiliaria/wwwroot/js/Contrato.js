document.addEventListener("DOMContentLoaded", function () {
    const inicioInput = document.querySelector("#FechaInicio");
    const finInput = document.querySelector("#FechaFin");

    if (inicioInput && finInput) {
        inicioInput.addEventListener("change", () => {
            const fechaInicio = new Date(inicioInput.value);
            if (!isNaN(fechaInicio)) {
                const fechaFin = new Date(fechaInicio);
                fechaFin.setFullYear(fechaFin.getFullYear() + 1);

                // Ajuste formato YYYY-MM-DD
                const yyyy = fechaFin.getFullYear();
                const mm = (fechaFin.getMonth() + 1).toString().padStart(2, '0');
                const dd = fechaFin.getDate().toString().padStart(2, '0');
                finInput.value = `${yyyy}-${mm}-${dd}`;
            }
        });
    }
});
