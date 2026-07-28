// Formulario de venta rápida (Facturas/VentaRapida): líneas de servicio
// dinámicas con precio editable, y la caja (método de pago, monto recibido,
// devuelta). El servidor siempre revalida (el monto recibido no puede ser
// menor al total); esto es solo para que la recepcionista vea los números
// antes de confirmar.
(function () {
    var lineasContainer = document.getElementById("lineasContainer");
    var agregarBtn = document.getElementById("agregarLineaBtn");
    var template = document.getElementById("lineaTemplate");
    var descuentoInput = document.getElementById("descuentoInput");
    var subtotalPreview = document.getElementById("subtotalPreview");
    var descuentoPreview = document.getElementById("descuentoPreview");
    var totalPreview = document.getElementById("totalPreview");
    var clienteSelect = document.getElementById("clienteSelect");
    var telefonoWrapper = document.getElementById("telefonoContactoWrapper");
    var metodoPagoSelect = document.getElementById("metodoPagoSelect");
    var montoRecibidoWrapper = document.getElementById("montoRecibidoWrapper");
    var montoRecibidoInput = document.getElementById("montoRecibidoInput");
    var devueltaWrapper = document.getElementById("devueltaWrapper");
    var devueltaPreview = document.getElementById("devueltaPreview");

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
        var precioInput = fila.querySelector(".linea-precio");
        var cantidadInput = fila.querySelector(".linea-cantidad");
        var subtotalDiv = fila.querySelector(".linea-subtotal");
        var precio = parseFloat(precioInput.value) || 0;
        var cantidad = parseFloat(cantidadInput.value) || 0;
        var subtotal = precio * cantidad;

        subtotalDiv.textContent = "$" + subtotal.toFixed(2);
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
        descuentoPreview.textContent = "-$" + descuento.toFixed(2);
        totalPreview.textContent = "$" + total.toFixed(2);

        actualizarDevuelta(total);
    }

    function actualizarDevuelta(total) {
        var esEfectivo = metodoPagoSelect.value === "Efectivo";
        montoRecibidoWrapper.style.display = esEfectivo ? "" : "none";
        devueltaWrapper.style.display = esEfectivo ? "" : "none";

        if (!esEfectivo) {
            return;
        }

        var montoRecibido = parseFloat(montoRecibidoInput.value) || 0;
        var devuelta = montoRecibido - total;
        devueltaPreview.textContent = "$" + Math.abs(devuelta).toFixed(2);
        devueltaWrapper.classList.toggle("elara-caja-devuelta-insuficiente", devuelta < 0);
    }

    function agregarListenersFila(fila) {
        fila.querySelectorAll(".linea-precio, .linea-cantidad").forEach(function (campo) {
            campo.addEventListener("input", recalcularTotales);
        });

        var servicioSelect = fila.querySelector(".linea-servicio");
        var precioInput = fila.querySelector(".linea-precio");
        servicioSelect.addEventListener("change", function () {
            var opcion = servicioSelect.options[servicioSelect.selectedIndex];
            precioInput.value = opcion ? (parseFloat(opcion.getAttribute("data-precio")) || 0).toFixed(2) : "0.00";
            recalcularTotales();
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
    metodoPagoSelect.addEventListener("change", recalcularTotales);
    montoRecibidoInput.addEventListener("input", recalcularTotales);

    if (clienteSelect && telefonoWrapper) {
        var actualizarTelefonoWrapper = function () {
            telefonoWrapper.style.display = clienteSelect.value === "" ? "" : "none";
        };
        clienteSelect.addEventListener("change", actualizarTelefonoWrapper);
        actualizarTelefonoWrapper();
    }

    recalcularTotales();
})();
