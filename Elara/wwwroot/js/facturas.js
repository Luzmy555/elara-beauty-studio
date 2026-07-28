// Vista previa en vivo del total (Subtotal - Descuento) en el formulario de
// generación de factura. El total real siempre lo recalcula el servidor;
// esto es solo para que la recepcionista vea el número antes de emitir.
(function () {
    var descuentoInput = document.getElementById("descuentoInput");
    var totalPreview = document.getElementById("totalPreview");
    var subtotalInput = document.getElementById("subtotalInput");

    if (!descuentoInput || !totalPreview || !subtotalInput) {
        return;
    }

    var subtotal = parseFloat(subtotalInput.value) || 0;

    descuentoInput.addEventListener("input", function () {
        var descuento = parseFloat(descuentoInput.value) || 0;
        var total = Math.max(subtotal - descuento, 0);
        totalPreview.textContent = "$" + total.toFixed(2);
    });
})();
