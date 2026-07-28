// Búsqueda en tiempo real (AJAX, sin recargar la página) y paginación
// del listado de clientes. El servidor devuelve un partial HTML
// (_ClienteCards) que reemplaza el contenido del contenedor.
(function () {
    var searchInput = document.getElementById("clienteSearchInput");
    var container = document.getElementById("clientesContainer");
    var searchUrl = "/Clientes/Buscar";
    var debounceTimer = null;

    if (!searchInput || !container) {
        return;
    }

    function cargarClientes(term, page) {
        var url = searchUrl + "?term=" + encodeURIComponent(term || "") + "&page=" + (page || 1);
        fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (response) { return response.text(); })
            .then(function (html) {
                container.innerHTML = html;
            });
    }

    searchInput.addEventListener("input", function () {
        clearTimeout(debounceTimer);
        var term = searchInput.value;
        debounceTimer = setTimeout(function () {
            cargarClientes(term, 1);
        }, 350);
    });

    // Delegación de eventos: los enlaces de paginación se recrean en cada
    // respuesta AJAX, así que el listener vive en el contenedor padre.
    container.addEventListener("click", function (event) {
        var link = event.target.closest(".js-page-link");
        if (!link) {
            return;
        }
        event.preventDefault();
        var page = parseInt(link.getAttribute("data-page"), 10) || 1;
        cargarClientes(searchInput.value, page);
    });
})();
