// Listado manual de Citas (sin calendario) compartido por dos vistas:
// - Citas/Index (Administrador/Recepcionista): crea, edita y reagenda citas.
// - Agenda/Index (Especialista): solo lectura + cambiar estado de sus citas.
// El comportamiento se ajusta según window.elaraCitasConfig, que cada
// vista define antes de cargar este script.
(function () {
    var config = window.elaraCitasConfig;
    if (!config) {
        return;
    }

    var listaCitas = document.getElementById("listaCitas");
    var fechaInputNav = document.getElementById("fechaSeleccionada");
    var modalEl = document.getElementById("citaModal");
    var modal = modalEl && window.bootstrap ? new bootstrap.Modal(modalEl) : null;
    var modalBody = document.getElementById("citaModalBody");
    var modalTitle = document.getElementById("citaModalTitle");

    // Definido en site.js (se carga en todas las vistas): agrega el header
    // que evita que un túnel de ngrok intercepte la petición con su propia
    // página de aviso en vez de dejarla pasar al servidor real.
    var fetchApp = window.elaraFetch || fetch;

    function obtenerTokenGlobal() {
        var tokenInput = document.querySelector('#globalAntiForgeryForm input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }

    function mostrarToast(mensaje, tipo) {
        var contenedor = document.getElementById("elaraToasts");
        if (!contenedor || !window.bootstrap) {
            return;
        }

        var toastEl = document.createElement("div");
        toastEl.className = "toast align-items-center border-0 mb-2 " + (tipo === "error" ? "elara-toast-error" : "elara-toast-success");
        toastEl.setAttribute("role", "alert");
        toastEl.innerHTML =
            '<div class="d-flex">' +
                '<div class="toast-body">' + mensaje + '</div>' +
                '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
            '</div>';
        contenedor.appendChild(toastEl);

        var toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();
        toastEl.addEventListener("hidden.bs.toast", function () { toastEl.remove(); });
    }

    // Los inputs date/datetime-local esperan la hora LOCAL de quien los usa.
    // Date.toISOString() siempre devuelve UTC, así que usarlo aquí adelantaba
    // (o atrasaba) la fecha/hora según el huso horario del navegador.
    function formatearFechaLocal(fecha) {
        var pad = function (n) { return n < 10 ? "0" + n : "" + n; };
        return fecha.getFullYear() + "-" + pad(fecha.getMonth() + 1) + "-" + pad(fecha.getDate()) +
            "T" + pad(fecha.getHours()) + ":" + pad(fecha.getMinutes());
    }

    function formatearHora(fechaIso) {
        var d = new Date(fechaIso);
        var pad = function (n) { return n < 10 ? "0" + n : "" + n; };
        return pad(d.getHours()) + ":" + pad(d.getMinutes());
    }

    var BadgePorEstado = {
        Pendiente: "text-bg-warning",
        Confirmada: "text-bg-info",
        EnProceso: "text-bg-primary",
        Completada: "text-bg-dark",
        Cancelada: "text-bg-danger",
        NoAsistio: "text-bg-secondary"
    };

    // ---- Listado del día ----

    var fechaSeleccionada = new Date();
    fechaSeleccionada.setHours(0, 0, 0, 0);

    function renderizarCitas(citas) {
        if (!listaCitas) { return; }
        listaCitas.innerHTML = "";

        if (!citas.length) {
            listaCitas.innerHTML = '<p class="text-elara-muted text-center py-4 mb-0">No hay citas para este día.</p>';
            return;
        }

        citas.sort(function (a, b) { return new Date(a.start) - new Date(b.start); });

        citas.forEach(function (item) {
            var fila = document.createElement("div");
            fila.className = "d-flex flex-wrap justify-content-between align-items-center gap-2 py-2 border-bottom elara-cita-fila";
            fila.setAttribute("role", "button");
            fila.tabIndex = 0;

            var botones = "";
            if (config.puedeCrear && item.puedeEditar) {
                botones = '<button type="button" class="btn btn-outline-elara btn-sm js-reagendar"><i class="bi bi-clock-history me-1"></i>Reagendar</button>';
            }

            fila.innerHTML =
                '<div>' +
                    '<div><strong>' + formatearHora(item.start) + ' - ' + formatearHora(item.end) + '</strong> ' +
                        '<span class="badge ' + (BadgePorEstado[item.estado] || "text-bg-light") + ' ms-1">' + item.estado + '</span></div>' +
                    '<div class="text-elara-muted small">' + item.clienteNombre + ' · ' + item.servicioNombre + ' · ' + item.empleadoNombre + '</div>' +
                '</div>' +
                '<div class="d-flex gap-2">' + botones + '</div>';

            fila.addEventListener("click", function (e) {
                if (e.target.closest(".js-reagendar")) { return; }
                if (config.puedeCrear && item.puedeEditar) {
                    abrirFormularioEdicion(item.id);
                } else {
                    abrirPanelEstado(item);
                }
            });

            var btnReagendar = fila.querySelector(".js-reagendar");
            if (btnReagendar) {
                btnReagendar.addEventListener("click", function (e) {
                    e.stopPropagation();
                    abrirReagendar(item);
                });
            }

            listaCitas.appendChild(fila);
        });
    }

    function cargarCitasDelDia() {
        if (!listaCitas) { return; }
        listaCitas.innerHTML = '<p class="text-elara-muted text-center py-4 mb-0">Cargando...</p>';

        var inicio = new Date(fechaSeleccionada);
        var fin = new Date(fechaSeleccionada);
        fin.setDate(fin.getDate() + 1);

        fetchApp(config.eventosUrl + "?start=" + formatearFechaLocal(inicio) + "&end=" + formatearFechaLocal(fin))
            .then(function (r) { return r.json(); })
            .then(renderizarCitas)
            .catch(function () {
                listaCitas.innerHTML = '<p class="text-elara-accent text-center py-4 mb-0">Ocurrió un error al cargar las citas.</p>';
            });
    }

    function irAFecha(fecha) {
        fechaSeleccionada = fecha;
        fechaSeleccionada.setHours(0, 0, 0, 0);
        if (fechaInputNav) {
            fechaInputNav.value = formatearFechaLocal(fechaSeleccionada).slice(0, 10);
        }
        cargarCitasDelDia();
    }

    var btnFechaAnterior = document.getElementById("btnFechaAnterior");
    var btnFechaHoy = document.getElementById("btnFechaHoy");
    var btnFechaSiguiente = document.getElementById("btnFechaSiguiente");

    if (btnFechaAnterior) {
        btnFechaAnterior.addEventListener("click", function () {
            var nueva = new Date(fechaSeleccionada);
            nueva.setDate(nueva.getDate() - 1);
            irAFecha(nueva);
        });
    }
    if (btnFechaSiguiente) {
        btnFechaSiguiente.addEventListener("click", function () {
            var nueva = new Date(fechaSeleccionada);
            nueva.setDate(nueva.getDate() + 1);
            irAFecha(nueva);
        });
    }
    if (btnFechaHoy) {
        btnFechaHoy.addEventListener("click", function () {
            irAFecha(new Date());
        });
    }
    if (fechaInputNav) {
        fechaInputNav.value = formatearFechaLocal(fechaSeleccionada).slice(0, 10);
        fechaInputNav.addEventListener("change", function () {
            if (!fechaInputNav.value) { return; }
            var partes = fechaInputNav.value.split("-");
            irAFecha(new Date(parseInt(partes[0], 10), parseInt(partes[1], 10) - 1, parseInt(partes[2], 10)));
        });
    }

    var btnNuevaCita = document.getElementById("btnNuevaCita");
    if (btnNuevaCita) {
        btnNuevaCita.addEventListener("click", function () {
            abrirFormularioCreacion(formatearFechaLocal(fechaSeleccionada).slice(0, 10));
        });
    }

    if (listaCitas) {
        cargarCitasDelDia();
    }

    // ---- Horarios disponibles (servicio + fecha + especialista) ----

    function cargarHorarios(servicioId, empleadoId, fecha, citaId, selectEl, preseleccionar, incluirActualSiFalta) {
        if (!servicioId || !empleadoId || !fecha) {
            selectEl.innerHTML = '<option value="">Selecciona servicio, fecha y especialista...</option>';
            selectEl.disabled = true;
            return;
        }

        var url = config.horariosDisponiblesUrl + "?servicioId=" + servicioId + "&empleadoId=" + empleadoId + "&fecha=" + encodeURIComponent(fecha);
        if (citaId) {
            url += "&citaId=" + citaId;
        }

        fetchApp(url)
            .then(function (r) { return r.json(); })
            .then(function (horas) {
                if (incluirActualSiFalta && preseleccionar && horas.indexOf(preseleccionar) === -1) {
                    horas = horas.concat([preseleccionar]).sort();
                }
                if (!horas.length) {
                    selectEl.innerHTML = '<option value="">Sin horarios disponibles ese día</option>';
                    selectEl.disabled = true;
                    return;
                }
                selectEl.innerHTML = '<option value="">Selecciona una hora...</option>';
                horas.forEach(function (h) {
                    var opt = document.createElement("option");
                    opt.value = h;
                    opt.textContent = h;
                    selectEl.appendChild(opt);
                });
                selectEl.disabled = false;
                if (preseleccionar) {
                    selectEl.value = preseleccionar;
                }
            });
    }

    // ---- Modal: nueva cita / editar cita ----

    // Si la sesión expiró (o el usuario perdió permisos), el servidor
    // redirige estas peticiones a la pantalla de login: sin este chequeo, el
    // modal terminaba mostrando el HTML crudo del login por dentro, lo que
    // parecía "no hacer nada". Se detecta y se avisa en vez de eso.
    function sesionExpirada(response) {
        return response.redirected && response.url.indexOf("/Account/Login") !== -1;
    }

    function cargarFormularioEnModal(url, titulo) {
        if (!modal) { return; }
        modalTitle.textContent = titulo;
        fetchApp(url)
            .then(function (r) {
                if (sesionExpirada(r)) {
                    mostrarToast("Tu sesión expiró. Recarga la página e inicia sesión de nuevo.", "error");
                    return null;
                }
                if (!r.ok) {
                    mostrarToast("No se pudo cargar el formulario. Intenta de nuevo.", "error");
                    return null;
                }
                return r.text();
            })
            .then(function (html) {
                if (html == null) { return; }

                // Si algo entre el navegador y el servidor (un túnel como
                // ngrok, un proxy, un portal de wifi) devuelve una página
                // distinta a la esperada, esto evita abrir el modal con
                // contenido irreconocible: se valida que el formulario real
                // venga incluido antes de mostrarlo.
                if (html.indexOf('id="citaForm"') === -1) {
                    mostrarToast("La respuesta del servidor no fue la esperada. Recarga la página e intenta de nuevo.", "error");
                    return;
                }

                modalBody.innerHTML = html;
                inicializarFormulario();
                modal.show();
            })
            .catch(function () {
                mostrarToast("Ocurrió un error de conexión.", "error");
            });
    }

    function abrirFormularioCreacion(fechaStr) {
        cargarFormularioEnModal(config.formularioCreacionUrl + "?fecha=" + encodeURIComponent(fechaStr), "Nueva cita");
    }

    function abrirFormularioEdicion(id) {
        cargarFormularioEnModal(config.formularioEdicionUrl + "/" + id, "Editar cita");
    }

    function inicializarFormulario() {
        var form = document.getElementById("citaForm");
        if (!form) { return; }

        var clienteInput = document.getElementById("clienteInput");
        var clienteIdInput = document.getElementById("ClienteId");
        var resultados = document.getElementById("clienteResultados");
        var servicioSelect = document.getElementById("ServicioId");
        var fechaInput = document.getElementById("Fecha");
        var empleadoSelect = document.getElementById("EmpleadoId");
        var horaSelect = document.getElementById("Hora");
        var fechaHoraHidden = document.getElementById("FechaHoraInicio");
        var servicioInfo = document.getElementById("servicioInfo");
        var errorBox = document.getElementById("citaFormError");
        var esEdicion = form.getAttribute("data-es-edicion") === "true";
        var citaIdActual = form.querySelector('input[name="Id"]').value;

        var debounceTimer = null;
        clienteInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            var term = clienteInput.value;
            clienteIdInput.value = "";
            if (term.length < 2) {
                resultados.style.display = "none";
                return;
            }
            debounceTimer = setTimeout(function () {
                fetchApp("/Citas/BuscarClientes?term=" + encodeURIComponent(term))
                    .then(function (r) { return r.json(); })
                    .then(function (clientes) {
                        resultados.innerHTML = "";
                        if (!clientes.length) {
                            resultados.style.display = "none";
                            return;
                        }
                        clientes.forEach(function (c) {
                            var item = document.createElement("button");
                            item.type = "button";
                            item.className = "list-group-item list-group-item-action";
                            item.textContent = c.nombreCompleto + " · " + c.telefono;
                            item.addEventListener("click", function () {
                                clienteInput.value = c.nombreCompleto;
                                clienteIdInput.value = c.id;
                                resultados.style.display = "none";
                            });
                            resultados.appendChild(item);
                        });
                        resultados.style.display = "block";
                    });
            }, 300);
        });

        document.addEventListener("click", function (e) {
            if (resultados && !resultados.contains(e.target) && e.target !== clienteInput) {
                resultados.style.display = "none";
            }
        });

        function actualizarHoras() {
            fechaHoraHidden.value = "";
            cargarHorarios(servicioSelect.value, empleadoSelect.value, fechaInput.value, esEdicion ? citaIdActual : null, horaSelect);
        }

        servicioSelect.addEventListener("change", function () {
            var opt = servicioSelect.selectedOptions[0];
            if (opt && opt.value) {
                servicioInfo.textContent = "Duración: " + opt.getAttribute("data-duracion") + " min · Precio: $" + opt.getAttribute("data-precio");
            } else {
                servicioInfo.textContent = "";
            }
            actualizarHoras();
        });

        fechaInput.addEventListener("change", actualizarHoras);
        empleadoSelect.addEventListener("change", actualizarHoras);

        horaSelect.addEventListener("change", function () {
            fechaHoraHidden.value = (fechaInput.value && horaSelect.value) ? (fechaInput.value + "T" + horaSelect.value) : "";
        });

        if (esEdicion) {
            var opt = servicioSelect.selectedOptions[0];
            if (opt && opt.value) {
                servicioInfo.textContent = "Duración: " + opt.getAttribute("data-duracion") + " min · Precio: $" + opt.getAttribute("data-precio");
            }
            // La hora guardada puede no caer en la grilla de 30 min (citas
            // creadas antes de este listado, o reagendadas con otro
            // intervalo): si no aparece entre las disponibles, se agrega
            // igual para no perderla si el usuario no cambia la hora.
            var horaActualAttr = horaSelect.getAttribute("data-hora-actual");
            cargarHorarios(servicioSelect.value, empleadoSelect.value, fechaInput.value, citaIdActual, horaSelect, horaActualAttr, true);
            fechaHoraHidden.value = fechaInput.value + "T" + horaActualAttr;
        }

        form.addEventListener("submit", function (event) {
            event.preventDefault();
            errorBox.textContent = "";

            if (!clienteIdInput.value) {
                errorBox.textContent = "Selecciona un cliente de la lista.";
                return;
            }
            if (!fechaHoraHidden.value) {
                errorBox.textContent = "Selecciona fecha, especialista y hora.";
                return;
            }

            var formData = new FormData(form);
            fetchApp("/Citas/Guardar", { method: "POST", body: formData })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success) {
                        modal.hide();
                        mostrarToast(esEdicion ? "Cita actualizada correctamente." : "Cita creada correctamente.", "success");
                        cargarCitasDelDia();
                    } else {
                        errorBox.textContent = data.error || "No se pudo guardar la cita.";
                    }
                })
                .catch(function () {
                    errorBox.textContent = "Ocurrió un error de conexión.";
                });
        });
    }

    // ---- Modal: reagendar (fila de la lista) ----

    function abrirReagendar(item) {
        if (!modal) { return; }
        modalTitle.textContent = "Reagendar cita";

        var fechaActual = formatearFechaLocal(new Date(item.start)).slice(0, 10);

        modalBody.innerHTML =
            '<p class="mb-3"><strong>' + item.clienteNombre + '</strong> · ' + item.servicioNombre + ' (' + item.empleadoNombre + ')</p>' +
            '<div class="mb-3">' +
                '<label class="form-label">Nueva fecha</label>' +
                '<input type="date" id="reagendarFecha" class="form-control" value="' + fechaActual + '" />' +
            '</div>' +
            '<div class="mb-3">' +
                '<label class="form-label">Nueva hora</label>' +
                '<select id="reagendarHora" class="form-select" disabled><option value="">Selecciona una fecha...</option></select>' +
            '</div>' +
            '<div id="citaFormError" class="text-elara-accent small mb-3"></div>' +
            '<div class="d-flex justify-content-end gap-2">' +
                '<button type="button" class="btn btn-outline-elara" data-bs-dismiss="modal">Cancelar</button>' +
                '<button type="button" class="btn btn-elara" id="btnConfirmarReagendar"><i class="bi bi-check2 me-1"></i> Reagendar</button>' +
            '</div>';

        var fechaInput = document.getElementById("reagendarFecha");
        var horaSelect = document.getElementById("reagendarHora");

        function actualizarHoras() {
            cargarHorarios(item.servicioId, item.empleadoId, fechaInput.value, item.id, horaSelect);
        }

        fechaInput.addEventListener("change", actualizarHoras);
        actualizarHoras();

        document.getElementById("btnConfirmarReagendar").addEventListener("click", function () {
            var errorBox = document.getElementById("citaFormError");
            if (!horaSelect.value) {
                errorBox.textContent = "Selecciona una hora.";
                return;
            }

            var formData = new FormData();
            formData.append("id", item.id);
            formData.append("inicio", fechaInput.value + "T" + horaSelect.value);
            formData.append("__RequestVerificationToken", obtenerTokenGlobal());

            fetchApp(config.reagendarUrl, { method: "POST", body: formData })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success) {
                        modal.hide();
                        mostrarToast("Cita reagendada correctamente.", "success");
                        cargarCitasDelDia();
                    } else {
                        errorBox.textContent = data.error || "No se pudo reagendar la cita.";
                    }
                })
                .catch(function () {
                    errorBox.textContent = "Ocurrió un error de conexión.";
                });
        });

        modal.show();
    }

    // ---- Modal: detalle / cambiar estado ----

    function abrirPanelEstado(item) {
        if (!modal) { return; }
        modalTitle.textContent = "Detalle de la cita";

        var transiciones = {
            Pendiente: [["Confirmada", "Confirmar"], ["Cancelada", "Cancelar"]],
            Confirmada: [["EnProceso", "Iniciar"], ["NoAsistio", "No asistió"], ["Cancelada", "Cancelar"]],
            EnProceso: [["Completada", "Completar"], ["Cancelada", "Cancelar"]]
        };

        var opciones = transiciones[item.estado] || [];
        var botones = opciones.map(function (op) {
            return '<button type="button" class="btn btn-elara btn-sm me-2 mb-2 js-cambiar-estado" data-estado="' + op[0] + '">' + op[1] + '</button>';
        }).join("");

        // La facturación es exclusiva de Administrador/Recepcionista
        // (config.puedeCrear); el Especialista nunca ve este botón en su Agenda.
        var botonFactura = "";
        if (config.puedeCrear && item.estado === "Completada") {
            botonFactura = item.facturaId
                ? '<a href="/Facturas/Details/' + item.facturaId + '" class="btn btn-elara btn-sm me-2 mb-2"><i class="bi bi-receipt me-1"></i>Ver factura</a>'
                : '<a href="/Facturas/Create?citaId=' + item.id + '" class="btn btn-elara btn-sm me-2 mb-2"><i class="bi bi-receipt me-1"></i>Generar factura</a>';
        }

        modalBody.innerHTML =
            '<p class="mb-1"><strong>Cliente:</strong> ' + item.clienteNombre + '</p>' +
            '<p class="mb-1"><strong>Servicio:</strong> ' + item.servicioNombre + '</p>' +
            '<p class="mb-3"><strong>Estado actual:</strong> ' + item.estado + '</p>' +
            (botones || botonFactura || '<p class="text-elara-muted">Esta cita ya está en un estado final.</p>') +
            '<div id="citaFormError" class="text-elara-accent small mt-2"></div>';

        modalBody.querySelectorAll(".js-cambiar-estado").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var nuevoEstado = boton.getAttribute("data-estado");
                var formData = new FormData();
                formData.append("id", item.id);
                formData.append("estado", nuevoEstado);
                formData.append("__RequestVerificationToken", obtenerTokenGlobal());

                fetchApp(config.cambiarEstadoUrl, { method: "POST", body: formData })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            modal.hide();
                            mostrarToast("Estado actualizado a \"" + nuevoEstado + "\".", "success");
                            cargarCitasDelDia();
                        } else {
                            document.getElementById("citaFormError").textContent = data.error || "No se pudo actualizar el estado.";
                        }
                    });
            });
        });

        modal.show();
    }
})();
