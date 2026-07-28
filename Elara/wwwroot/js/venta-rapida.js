// Formulario de venta rápida (Facturas/VentaRapida): líneas de servicio
// dinámicas y vista previa de subtotal/total. El servidor siempre recalcula
// los precios reales al emitir; esto es solo para que la recepcionista vea
// el número antes de confirmar.
(function () {
    var lineasContainer = document.getElementById("lineasContainer");
    var agregarBtn = document.getElementById("agregarLineaBtn");
    var template = document.getElementById("lineaTemplate");
    var descuentoInput = document.getElementById("descuentoInput");
    var subtotalPreview = document.getElementById("subtotalPreview");
    var totalPreview = document.getElementById("totalPreview");
    var clienteSelect = document.getElementById("clienteSelect");
    var telefonoWrapper = document.getElementById("telefonoContactoWrapper");

    if (!lineasContainer || !template) {
        return;
    }

    function reindexar() {
        var filas = lineasContainer.querySelectorAll(".linea-row");
        filas.forEach(function (fila, index) {
            fila.querySelectorAll("[name]").forEach(function (campo) {
                campo.name = campo.name.replace(/Lineas\[[^\]]*\]/, "Lineas[" + index + "]");
            });
        });

        var botones = lineasContainer.querySelectorAll(".quitar-linea-btn");
        botones.forEach(function (boton) {
            boton.disabled = filas.length <= 1;
        });
    }

    function recalcularLinea(fila) {
        var servicioSelect = fila.querySelector(".linea-servicio");
        var cantidadInput = fila.querySelector(".linea-cantidad");
        var subtotalSpan = fila.querySelector(".linea-subtotal");
        var opcion = servicioSelect.options[servicioSelect.selectedIndex];
        var precio = opcion ? parseFloat(opcion.getAttribute("data-precio")) || 0 : 0;
        var cantidad = parseFloat(cantidadInput.value) || 0;
        var subtotal = precio * cantidad;

        subtotalSpan.textContent = "$" + subtotal.toFixed(2);
        return subtotal;
    }

    function recalcularTotales() {
        var subtotalGeneral = 0;
        lineasContainer.querySelectorAll(".linea-row").forEach(function (fila) {
            subtotalGeneral += recalcularLinea(fila);
        });

        var descuento = parseFloat(descuentoInput.value) || 0;
        var total = Math.max(subtotalGeneral - descuento, 0);

        subtotalPreview.textContent = "$" + subtotalGeneral.toFixed(2);
        totalPreview.textContent = "$" + total.toFixed(2);
    }

    function agregarListenersFila(fila) {
        fila.querySelectorAll(".linea-servicio, .linea-cantidad").forEach(function (campo) {
            campo.addEventListener("input", recalcularTotales);
            campo.addEventListener("change", recalcularTotales);
        });

        fila.querySelector(".quitar-linea-btn").addEventListener("click", function () {
            if (lineasContainer.querySelectorAll(".linea-row").length <= 1) {
                return;
            }
            fila.remove();
            reindexar();
            recalcularTotales();
        });
    }

    lineasContainer.querySelectorAll(".linea-row").forEach(agregarListenersFila);
    reindexar();

    agregarBtn.addEventListener("click", function () {
        lineasContainer.appendChild(template.content.cloneNode(true));
        reindexar();
        agregarListenersFila(lineasContainer.querySelector(".linea-row:last-child"));
        recalcularTotales();
    });

    descuentoInput.addEventListener("input", recalcularTotales);

    if (clienteSelect && telefonoWrapper) {
        var actualizarTelefonoWrapper = function () {
            telefonoWrapper.style.display = clienteSelect.value === "" ? "" : "none";
        };
        clienteSelect.addEventListener("change", actualizarTelefonoWrapper);
        actualizarTelefonoWrapper();
    }

    recalcularTotales();
})();
