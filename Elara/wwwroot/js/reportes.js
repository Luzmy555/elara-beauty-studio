// Dashboard de Reportes: pide los 4 gráficos como JSON al controller y los
// dibuja con Chart.js (CDN). El filtro de fechas re-consulta los 4 a la vez
// y actualiza el link de exportación a Excel.
(function () {
    var config = window.elaraReportesConfig;
    if (!config || !window.Chart) {
        return;
    }

    var paletteDonut = ["#C9A15A", "#A8B79A", "#C77B6C", "#8A7F73", "#E4C688", "#2B2620", "#D9C7B8", "#7C8F6E"];

    var desdeInput = document.getElementById("filtroDesde");
    var hastaInput = document.getElementById("filtroHasta");
    var form = document.getElementById("filtroFechasForm");
    var btnExportar = document.getElementById("btnExportarExcel");

    var chartIngresos = null;
    var chartServicios = null;
    var chartRanking = null;
    var chartClientes = null;

    var fetchApp = window.elaraFetch || fetch;

    function queryString() {
        return "?desde=" + encodeURIComponent(desdeInput.value) + "&hasta=" + encodeURIComponent(hastaInput.value);
    }

    function cargarIngresos() {
        fetchApp(config.ingresosUrl + queryString())
            .then(function (r) { return r.json(); })
            .then(function (datos) {
                var ctx = document.getElementById("chartIngresos");
                var labels = datos.map(function (d) { return d.etiqueta; });
                var valores = datos.map(function (d) { return d.valor; });

                if (chartIngresos) { chartIngresos.destroy(); }
                chartIngresos = new Chart(ctx, {
                    type: "bar",
                    data: {
                        labels: labels,
                        datasets: [{
                            label: "Ingresos ($)",
                            data: valores,
                            backgroundColor: "#C9A15A",
                            borderRadius: 6
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true } }
                    }
                });
            });
    }

    function cargarServicios() {
        fetchApp(config.serviciosUrl + queryString())
            .then(function (r) { return r.json(); })
            .then(function (datos) {
                var ctx = document.getElementById("chartServicios");
                var labels = datos.map(function (d) { return d.nombre; });
                var valores = datos.map(function (d) { return d.cantidad; });

                if (chartServicios) { chartServicios.destroy(); }
                chartServicios = new Chart(ctx, {
                    type: "doughnut",
                    data: {
                        labels: labels,
                        datasets: [{
                            data: valores,
                            backgroundColor: paletteDonut
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { position: "bottom" } }
                    }
                });
            });
    }

    function cargarRanking() {
        fetchApp(config.rankingUrl + queryString())
            .then(function (r) { return r.json(); })
            .then(function (datos) {
                var ctx = document.getElementById("chartRanking");
                var labels = datos.map(function (d) { return d.nombreCompleto; });
                var valores = datos.map(function (d) { return d.citasCompletadas; });

                if (chartRanking) { chartRanking.destroy(); }
                chartRanking = new Chart(ctx, {
                    type: "bar",
                    data: {
                        labels: labels,
                        datasets: [{
                            label: "Citas completadas",
                            data: valores,
                            backgroundColor: "#A8B79A",
                            borderRadius: 6
                        }]
                    },
                    options: {
                        indexAxis: "y",
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            });
    }

    function cargarClientes() {
        fetchApp(config.clientesUrl + queryString())
            .then(function (r) { return r.json(); })
            .then(function (datos) {
                var ctx = document.getElementById("chartClientes");
                var labels = datos.map(function (d) { return d.etiqueta; });
                var valores = datos.map(function (d) { return d.valor; });

                if (chartClientes) { chartClientes.destroy(); }
                chartClientes = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: labels,
                        datasets: [{
                            label: "Clientes nuevos",
                            data: valores,
                            borderColor: "#A8B79A",
                            backgroundColor: "rgba(168, 183, 154, 0.2)",
                            fill: true,
                            tension: 0.3
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            });
    }

    function cargarTodo() {
        cargarIngresos();
        cargarServicios();
        cargarRanking();
        cargarClientes();
    }

    function actualizarLinkExportar() {
        if (btnExportar) {
            btnExportar.setAttribute("href", config.exportarUrl + queryString());
        }
    }

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        cargarTodo();
        actualizarLinkExportar();
    });

    actualizarLinkExportar();
    cargarTodo();
})();
