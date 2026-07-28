// Filtro de categoría (pills) + búsqueda por nombre del catálogo de
// servicios: puramente client-side, oculta/muestra las cards ya renderizadas
// sin recargar ni pedir nada al servidor.
(function () {
    var filtros = document.querySelectorAll("#categoriaFiltros .elara-pill");
    var tarjetas = document.querySelectorAll("#serviciosGrid > [data-categoria]");
    var searchInput = document.getElementById("servicioSearchInput");
    var sinResultados = document.getElementById("sinResultadosServicios");

    if (!tarjetas.length) {
        return;
    }

    var categoriaActual = "todos";

    function aplicarFiltros() {
        var termino = (searchInput ? searchInput.value : "").trim().toLowerCase();
        var visibles = 0;

        tarjetas.forEach(function (tarjeta) {
            var coincideCategoria = categoriaActual === "todos" || tarjeta.getAttribute("data-categoria") === categoriaActual;
            var coincideNombre = !termino || (tarjeta.getAttribute("data-nombre") || "").indexOf(termino) !== -1;
            var visible = coincideCategoria && coincideNombre;

            tarjeta.style.display = visible ? "" : "none";
            if (visible) {
                visibles++;
            }
        });

        if (sinResultados) {
            sinResultados.style.display = visibles === 0 ? "" : "none";
        }
    }

    filtros.forEach(function (boton) {
        boton.addEventListener("click", function () {
            filtros.forEach(function (b) {
                b.classList.remove("btn-elara");
                b.classList.add("btn-outline-elara");
            });
            boton.classList.remove("btn-outline-elara");
            boton.classList.add("btn-elara");

            categoriaActual = boton.getAttribute("data-categoria");
            aplicarFiltros();
        });
    });

    if (searchInput) {
        searchInput.addEventListener("input", aplicarFiltros);
    }
})();
