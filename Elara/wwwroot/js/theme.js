// Toggle sol/luna del navbar: alterna data-theme en <html> (site.css hace la
// transición suave vía CSS) y, si hay sesión iniciada, persiste la preferencia
// en la BD (no en localStorage) para que se recuerde en cualquier dispositivo.
(function () {
    var root = document.documentElement;
    var toggleBtn = document.getElementById("themeToggle");

    if (!toggleBtn) {
        return;
    }

    var icon = toggleBtn.querySelector("i");

    function actualizarIcono(esOscuro) {
        if (!icon) {
            return;
        }
        icon.className = esOscuro ? "bi bi-sun fs-5" : "bi bi-moon-stars fs-5";
    }

    function obtenerToken() {
        var input = document.querySelector('#elaraAntiForgeryForm input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    toggleBtn.addEventListener("click", function () {
        var esOscuroNuevo = root.getAttribute("data-theme") !== "dark";

        root.setAttribute("data-theme", esOscuroNuevo ? "dark" : "light");
        actualizarIcono(esOscuroNuevo);

        if (toggleBtn.getAttribute("data-authenticated") === "true") {
            var formData = new FormData();
            formData.append("esOscuro", esOscuroNuevo);
            formData.append("__RequestVerificationToken", obtenerToken());

            fetch("/Theme/Establecer", { method: "POST", body: formData }).catch(function () {
                // El tema ya quedó aplicado en esta pantalla aunque falle el
                // guardado; se reintenta solo si el usuario vuelve a cambiarlo.
            });
        }
    });
})();
