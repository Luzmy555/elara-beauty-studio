// Habilita/deshabilita las horas de inicio y fin según si el día está
// marcado como "trabaja" en el formulario de Empleados, y maneja el cálculo
// (por ahora simulado) de comisión ganada por periodo en Details.
(function () {
    document.querySelectorAll(".js-dia-trabaja").forEach(function (checkbox) {
        var index = checkbox.getAttribute("data-index");
        var inicio = document.querySelector('.js-hora-inicio[data-index="' + index + '"]');
        var fin = document.querySelector('.js-hora-fin[data-index="' + index + '"]');

        function actualizar() {
            var habilitado = checkbox.checked;
            if (inicio) { inicio.disabled = !habilitado; }
            if (fin) { fin.disabled = !habilitado; }
        }

        checkbox.addEventListener("change", actualizar);
        actualizar();
    });

    var comisionForm = document.getElementById("comisionForm");
    if (comisionForm) {
        comisionForm.addEventListener("submit", function (event) {
            event.preventDefault();
            var empleadoId = comisionForm.getAttribute("data-empleado-id");
            var desde = comisionForm.querySelector('[name="desde"]').value;
            var hasta = comisionForm.querySelector('[name="hasta"]').value;
            var resultado = document.getElementById("comisionResultado");

            var url = "/Empleados/Comision/" + empleadoId +
                "?desde=" + encodeURIComponent(desde) +
                "&hasta=" + encodeURIComponent(hasta);

            fetch(url)
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    resultado.textContent = "Total estimado (" + data.desde + " - " + data.hasta + "): $" + data.total;
                });
        });
    }
})();
