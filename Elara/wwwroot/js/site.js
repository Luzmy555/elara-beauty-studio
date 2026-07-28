// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Wrapper de fetch() compartido por todas las páginas: agrega el header que
// le pide a un túnel de desarrollo (ngrok) que no intercepte la petición con
// su propia página de aviso. Sin esto, probar la app desde un celular a
// través de un link de ngrok podía devolver esa página en vez de la
// respuesta real, dejando formularios/modales vacíos o con contenido
// irreconocible. Se define en site.js (se carga en todas las vistas) para
// que cualquier script de página lo use en vez de fetch() directo.
window.elaraFetch = function (url, opciones) {
    opciones = opciones || {};
    var headers = new Headers(opciones.headers || {});
    headers.set("ngrok-skip-browser-warning", "true");
    opciones.headers = headers;
    return fetch(url, opciones);
};

// Toggle del sidebar del panel administrativo:
// - En pantallas >= 768px: alterna entre ancho completo (íconos + texto) y solo íconos.
// - En pantallas < 768px: alterna entre oculto (solo íconos) y expandido sobre el contenido.
(function () {
    var sidebar = document.getElementById("elaraSidebar");
    var toggleBtn = document.getElementById("sidebarToggle");
    var MOBILE_BREAKPOINT = 768;

    if (!sidebar || !toggleBtn) {
        return;
    }

    toggleBtn.addEventListener("click", function () {
        if (window.innerWidth < MOBILE_BREAKPOINT) {
            sidebar.classList.toggle("expanded");
        } else {
            sidebar.classList.toggle("collapsed");
        }
    });
})();
