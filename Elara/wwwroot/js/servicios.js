// Filtro de categoría (pills) del catálogo de servicios: puramente client-side,
// oculta/muestra las cards ya renderizadas sin recargar ni pedir nada al servidor.
(function () {
    var filtros = document.querySelectorAll("#categoriaFiltros .elara-pill");
    var tarjetas = document.querySelectorAll("#serviciosGrid > [data-categoria]");

    if (!filtros.length || !tarjetas.length) {
        return;
    }

    filtros.forEach(function (boton) {
        boton.addEventListener("click", function () {
            filtros.forEach(function (b) {
                b.classList.remove("btn-elara");
                b.classList.add("btn-outline-elara");
            });
            boton.classList.remove("btn-outline-elara");
            boton.classList.add("btn-elara");

            var categoria = boton.getAttribute("data-categoria");
            tarjetas.forEach(function (tarjeta) {
                var coincide = categoria === "todos" || tarjeta.getAttribute("data-categoria") === categoria;
                tarjeta.style.display = coincide ? "" : "none";
            });
        });
    });
})();
