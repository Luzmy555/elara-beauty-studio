// Vista previa en vivo del total (Subtotal - Descuento) y la devuelta en el
// formulario de generación de factura desde una cita. El servidor siempre
// recalcula todo; esto es solo para que la recepcionista vea los números
// antes de emitir.
(function () {
    var descuentoInput = document.getElementById("descuentoInput");
    var subtotalInput = document.getElementById("subtotalInput");
    var subtotalPreview = document.getElementById("subtotalPreview");
    var descuentoPreview = document.getElementById("descuentoPreview");
    var totalPreview = document.getElementById("totalPreview");
    var montoRecibidoInput = document.getElementById("montoRecibidoInput");
    var devueltaWrapper = document.getElementById("devueltaWrapper");
    var devueltaPreview = document.getElementById("devueltaPreview");

    if (!descuentoInput || !totalPreview || !subtotalInput) {
        return;
    }

    var subtotal = parseFloat(subtotalInput.value) || 0;
    subtotalPreview.textContent = "$" + subtotal.toFixed(2);

    function recalcular() {
        var descuento = parseFloat(descuentoInput.value) || 0;
        var total = Math.max(subtotal - descuento, 0);

        descuentoPreview.textContent = "-$" + descuento.toFixed(2);
        totalPreview.textContent = "$" + total.toFixed(2);

        actualizarDevuelta(total);
    }

    function actualizarDevuelta(total) {
        // El campo queda siempre visible, sin importar el método de pago: el
        // servidor igual ignora este valor si el pago no es en efectivo.
        if (!devueltaWrapper || !montoRecibidoInput) {
            return;
        }

        var montoRecibido = parseFloat(montoRecibidoInput.value) || 0;
        var devuelta = montoRecibido - total;
        // Negativo (con "-") cuando el monto recibido no alcanza a cubrir el
        // total, para que salte a la vista que falta cobrar esa diferencia.
        devueltaPreview.textContent = (devuelta < 0 ? "-$" : "$") + Math.abs(devuelta).toFixed(2);
        devueltaWrapper.classList.toggle("elara-caja-devuelta-insuficiente", devuelta < 0);
    }

    descuentoInput.addEventListener("input", recalcular);
    if (montoRecibidoInput) {
        montoRecibidoInput.addEventListener("input", recalcular);
    }

    recalcular();
})();
