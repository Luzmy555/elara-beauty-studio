// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

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
