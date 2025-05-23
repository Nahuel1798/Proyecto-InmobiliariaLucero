// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        const fechaInicioNueva = new Date(document.querySelector("#FechaInicio").value);
        const fechaFinActual = new Date(document.querySelector("#fechaFinActual").value);

        if (fechaInicioNueva <= fechaFinActual) {
            e.preventDefault();

            const errorDiv = document.querySelector("div[asp-validation-summary='ModelOnly']");
            errorDiv.innerHTML = "<div>No se puede renovar el contrato antes de que finalice el actual.</div>";
            errorDiv.classList.add("text-danger");
        }
    });
});

