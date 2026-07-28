// Formulario de venta rápida (Facturas/VentaRapida): líneas de servicio y de
// producto dinámicas con precio editable, y la caja (método de pago, monto
// recibido, devuelta, comprobante de transferencia). El servidor siempre
// revalida (stock disponible, monto recibido vs. total); esto es solo para
// que la recepcionista vea los números antes de confirmar.
(function () {
    var lineasContainer = document.getElementById("lineasContainer");
    var agregarBtn = document.getElementById("agregarLineaBtn");
    var template = document.getElementById("lineaTemplate");

    var lineasProductoContainer = document.getElementById("lineasProductoContainer");
    var agregarProductoBtn = document.getElementById("agregarLineaProductoBtn");
    var templateProducto = document.getElementById("lineaProductoTemplate");
    var sinProductosMensaje = document.getElementById("sinProductosMensaje");

    var descuentoInput = document.getElementById("descuentoInput");
    var subtotalPreview = document.getElementById("subtotalPreview");
    var descuentoPreview = document.getElementById("descuentoPreview");
    var totalPreview = document.getElementById("totalPreview");
    var clienteSelect = document.getElementById("clienteSelect");
    var telefonoWrapper = document.getElementById("telefonoContactoWrapper");
    var metodoPagoSelect = document.getElementById("metodoPagoSelect");
    var montoRecibidoInput = document.getElementById("montoRecibidoInput");
    var devueltaWrapper = document.getElementById("devueltaWrapper");
    var devueltaPreview = document.getElementById("devueltaPreview");
    var comprobanteWrapper = document.getElementById("comprobanteTransferenciaWrapper");

    if (!lineasContainer || !template) {
        return;
    }

    function reindexarGrupo(container, rowClass, fieldPrefix) {
        var filas = container.querySelectorAll("." + rowClass);
        filas.forEach(function (fila, index) {
            fila.querySelectorAll("[name]").forEach(function (campo) {
                campo.name = campo.name.replace(new RegExp(fieldPrefix + "\\[[^\\]]*\\]"), fieldPrefix + "[" + index + "]");
            });
        });
        return filas;
    }

    function actualizarBotonesQuitar(filas, btnClass, minimo) {
        filas.forEach(function (fila) {
            var boton = fila.querySelector("." + btnClass);
            if (boton) {
                boton.disabled = filas.length <= minimo;
            }
        });
    }

    function reindexar() {
        var filas = reindexarGrupo(lineasContainer, "linea-row", "Lineas");
        actualizarBotonesQuitar(filas, "quitar-linea-btn", 1);
    }

    function reindexarProductos() {
        var filas = reindexarGrupo(lineasProductoContainer, "linea-producto-row", "LineasProductos");
        actualizarBotonesQuitar(filas, "quitar-linea-producto-btn", 0);
        if (sinProductosMensaje) {
            sinProductosMensaje.style.display = filas.length === 0 ? "" : "none";
        }
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

    function recalcularLineaProducto(fila) {
        var precioInput = fila.querySelector(".linea-producto-precio");
        var cantidadInput = fila.querySelector(".linea-producto-cantidad");
        var subtotalDiv = fila.querySelector(".linea-producto-subtotal");
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
        lineasProductoContainer.querySelectorAll(".linea-producto-row").forEach(function (fila) {
            subtotalGeneral += recalcularLineaProducto(fila);
        });

        var descuento = parseFloat(descuentoInput.value) || 0;
        var total = Math.max(subtotalGeneral - descuento, 0);

        subtotalPreview.textContent = "$" + subtotalGeneral.toFixed(2);
        descuentoPreview.textContent = "-$" + descuento.toFixed(2);
        totalPreview.textContent = "$" + total.toFixed(2);

        actualizarDevuelta(total);
    }

    function actualizarDevuelta(total) {
        // El campo queda siempre visible (independiente del método de pago):
        // ocultarlo con JS según selección causaba que algunos navegadores
        // nunca lo mostraran. El servidor igual ignora este valor si el pago
        // no es en efectivo.
        if (comprobanteWrapper) {
            comprobanteWrapper.style.display = metodoPagoSelect.value === "Transferencia" ? "" : "none";
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

    function agregarListenersFilaProducto(fila) {
        fila.querySelectorAll(".linea-producto-precio, .linea-producto-cantidad").forEach(function (campo) {
            campo.addEventListener("input", recalcularTotales);
        });

        var productoSelect = fila.querySelector(".linea-producto-select");
        var precioInput = fila.querySelector(".linea-producto-precio");
        var cantidadInput = fila.querySelector(".linea-producto-cantidad");
        productoSelect.addEventListener("change", function () {
            var opcion = productoSelect.options[productoSelect.selectedIndex];
            precioInput.value = opcion ? (parseFloat(opcion.getAttribute("data-precio")) || 0).toFixed(2) : "0.00";
            var stock = opcion ? parseFloat(opcion.getAttribute("data-stock")) || 0 : 0;
            cantidadInput.max = stock || "";
            recalcularTotales();
        });

        fila.querySelector(".quitar-linea-producto-btn").addEventListener("click", function () {
            fila.remove();
            reindexarProductos();
            recalcularTotales();
        });
    }

    lineasContainer.querySelectorAll(".linea-row").forEach(agregarListenersFila);
    reindexar();

    lineasProductoContainer.querySelectorAll(".linea-producto-row").forEach(agregarListenersFilaProducto);
    reindexarProductos();

    agregarBtn.addEventListener("click", function () {
        lineasContainer.appendChild(template.content.cloneNode(true));
        reindexar();
        agregarListenersFila(lineasContainer.querySelector(".linea-row:last-child"));
        recalcularTotales();
    });

    agregarProductoBtn.addEventListener("click", function () {
        lineasProductoContainer.appendChild(templateProducto.content.cloneNode(true));
        reindexarProductos();
        agregarListenersFilaProducto(lineasProductoContainer.querySelector(".linea-producto-row:last-child"));
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
