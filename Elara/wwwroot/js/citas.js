// Calendario de Citas (FullCalendar vía CDN) compartido por dos vistas:
// - Citas/Index (Administrador/Recepcionista): crea, edita y arrastra citas.
// - Agenda/Index (Especialista): solo lectura + cambiar estado de sus citas.
// El comportamiento se ajusta según window.elaraCalendarioConfig, que cada
// vista define antes de cargar este script.
(function () {
    var config = window.elaraCalendarioConfig;
    if (!config) {
        return;
    }

    var calendarEl = document.getElementById("calendario");
    var modalEl = document.getElementById("citaModal");
    var modal = modalEl && window.bootstrap ? new bootstrap.Modal(modalEl) : null;
    var modalBody = document.getElementById("citaModalBody");
    var modalTitle = document.getElementById("citaModalTitle");
    var calendar = null;

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

    // En un celular una semana completa de columnas no cabe: arranca en
    // vista de un solo día y con menos botones en la barra superior.
    function esPantallaAngosta() {
        return window.matchMedia("(max-width: 575.98px)").matches;
    }

    if (calendarEl && window.FullCalendar) {
        calendar = new FullCalendar.Calendar(calendarEl, {
            locale: "es",
            height: "auto",
            headerToolbar: esPantallaAngosta()
                ? { left: "prev,next", center: "title", right: "today" }
                : { left: "prev,next today", center: "title", right: "dayGridMonth,timeGridWeek,timeGridDay" },
            footerToolbar: esPantallaAngosta()
                ? { left: "", center: "dayGridMonth,timeGridDay", right: "" }
                : false,
            initialView: esPantallaAngosta() ? "timeGridDay" : "timeGridWeek",
            slotMinTime: "07:00:00",
            slotMaxTime: "21:00:00",
            nowIndicator: true,
            editable: !!config.puedeArrastrar,
            events: function (info, successCallback, failureCallback) {
                fetch(config.eventosUrl + "?start=" + info.startStr + "&end=" + info.endStr)
                    .then(function (r) { return r.json(); })
                    .then(successCallback)
                    .catch(failureCallback);
            },
            dateClick: function (info) {
                if (config.puedeCrear) {
                    abrirFormularioCreacion(info.dateStr);
                }
            },
            eventClick: function (info) {
                var estado = info.event.extendedProps.estado;
                var esEstadoFinal = estado === "Completada" || estado === "Cancelada" || estado === "NoAsistio";

                // Las citas en estado final no se pueden editar (el servidor
                // lo rechaza), así que se muestra el panel de detalle/factura
                // en vez del formulario completo de edición.
                if (config.puedeCrear && !esEstadoFinal) {
                    abrirFormularioEdicion(info.event.id);
                } else {
                    abrirPanelEstado(info.event);
                }
            },
            eventDrop: function (info) {
                if (!config.puedeArrastrar) {
                    info.revert();
                    return;
                }
                reagendar(info.event.id, info.event.startStr, info.revert);
            }
        });
        calendar.render();
    }

    var btnNuevaCita = document.getElementById("btnNuevaCita");
    if (btnNuevaCita) {
        btnNuevaCita.addEventListener("click", function () {
            var ahora = new Date();
            ahora.setSeconds(0, 0);
            abrirFormularioCreacion(ahora.toISOString().slice(0, 16));
        });
    }

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
        fetch(url)
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
        var fechaInput = document.getElementById("FechaHoraInicio");
        var empleadoSelect = document.getElementById("EmpleadoId");
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
                fetch("/Citas/BuscarClientes?term=" + encodeURIComponent(term))
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

        function actualizarEmpleadosDisponibles(preseleccionar) {
            var servicioId = servicioSelect.value;
            var fecha = fechaInput.value;

            if (!servicioId || !fecha) {
                empleadoSelect.innerHTML = '<option value="">Selecciona servicio y horario primero...</option>';
                empleadoSelect.disabled = true;
                return;
            }

            var url = "/Citas/EmpleadosDisponibles?servicioId=" + servicioId + "&inicio=" + encodeURIComponent(fecha);
            if (esEdicion && citaIdActual) {
                url += "&citaId=" + citaIdActual;
            }

            fetch(url)
                .then(function (r) { return r.json(); })
                .then(function (empleados) {
                    if (!empleados.length) {
                        empleadoSelect.innerHTML = '<option value="">Sin especialistas disponibles en ese horario</option>';
                        empleadoSelect.disabled = true;
                        return;
                    }
                    empleadoSelect.innerHTML = '<option value="">Selecciona un especialista...</option>';
                    empleados.forEach(function (e) {
                        var opt = document.createElement("option");
                        opt.value = e.id;
                        opt.textContent = e.nombreCompleto;
                        empleadoSelect.appendChild(opt);
                    });
                    empleadoSelect.disabled = false;
                    if (preseleccionar) {
                        empleadoSelect.value = preseleccionar;
                    }
                });
        }

        servicioSelect.addEventListener("change", function () {
            var opt = servicioSelect.selectedOptions[0];
            if (opt && opt.value) {
                servicioInfo.textContent = "Duración: " + opt.getAttribute("data-duracion") + " min · Precio: $" + opt.getAttribute("data-precio");
            } else {
                servicioInfo.textContent = "";
            }
            actualizarEmpleadosDisponibles();
        });

        fechaInput.addEventListener("change", function () {
            actualizarEmpleadosDisponibles();
        });

        if (esEdicion) {
            var empleadoActual = empleadoSelect.getAttribute("data-actual");
            var opt = servicioSelect.selectedOptions[0];
            if (opt && opt.value) {
                servicioInfo.textContent = "Duración: " + opt.getAttribute("data-duracion") + " min · Precio: $" + opt.getAttribute("data-precio");
            }
            if (servicioSelect.value && fechaInput.value) {
                actualizarEmpleadosDisponibles(empleadoActual);
            }
        }

        form.addEventListener("submit", function (event) {
            event.preventDefault();
            errorBox.textContent = "";

            if (!clienteIdInput.value) {
                errorBox.textContent = "Selecciona un cliente de la lista.";
                return;
            }

            var formData = new FormData(form);
            fetch("/Citas/Guardar", { method: "POST", body: formData })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success) {
                        modal.hide();
                        mostrarToast(esEdicion ? "Cita actualizada correctamente." : "Cita creada correctamente.", "success");
                        if (calendar) { calendar.refetchEvents(); }
                    } else {
                        errorBox.textContent = data.error || "No se pudo guardar la cita.";
                    }
                })
                .catch(function () {
                    errorBox.textContent = "Ocurrió un error de conexión.";
                });
        });
    }

    function reagendar(id, nuevoInicioIso, revertirFn) {
        var formData = new FormData();
        formData.append("id", id);
        formData.append("inicio", nuevoInicioIso);
        formData.append("__RequestVerificationToken", obtenerTokenGlobal());

        fetch(config.reagendarUrl, { method: "POST", body: formData })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success) {
                    mostrarToast("Cita reagendada correctamente.", "success");
                } else {
                    mostrarToast(data.error || "No se pudo reagendar la cita.", "error");
                    revertirFn();
                }
            })
            .catch(function () {
                mostrarToast("Ocurrió un error de conexión.", "error");
                revertirFn();
            });
    }

    function abrirPanelEstado(evento) {
        if (!modal) { return; }
        var props = evento.extendedProps;
        modalTitle.textContent = "Detalle de la cita";

        var transiciones = {
            Pendiente: [["Confirmada", "Confirmar"], ["Cancelada", "Cancelar"]],
            Confirmada: [["EnProceso", "Iniciar"], ["NoAsistio", "No asistió"], ["Cancelada", "Cancelar"]],
            EnProceso: [["Completada", "Completar"], ["Cancelada", "Cancelar"]]
        };

        var opciones = transiciones[props.estado] || [];
        var botones = opciones.map(function (op) {
            return '<button type="button" class="btn btn-elara btn-sm me-2 mb-2 js-cambiar-estado" data-estado="' + op[0] + '">' + op[1] + '</button>';
        }).join("");

        // La facturación es exclusiva de Administrador/Recepcionista
        // (config.puedeCrear); el Especialista nunca ve este botón en su Agenda.
        var botonFactura = "";
        if (config.puedeCrear && props.estado === "Completada") {
            botonFactura = props.facturaId
                ? '<a href="/Facturas/Details/' + props.facturaId + '" class="btn btn-elara btn-sm me-2 mb-2"><i class="bi bi-receipt me-1"></i>Ver factura</a>'
                : '<a href="/Facturas/Create?citaId=' + evento.id + '" class="btn btn-elara btn-sm me-2 mb-2"><i class="bi bi-receipt me-1"></i>Generar factura</a>';
        }

        modalBody.innerHTML =
            '<p class="mb-1"><strong>Cliente:</strong> ' + props.cliente + '</p>' +
            '<p class="mb-1"><strong>Servicio:</strong> ' + props.servicio + '</p>' +
            '<p class="mb-3"><strong>Estado actual:</strong> ' + props.estado + '</p>' +
            (botones || botonFactura || '<p class="text-elara-muted">Esta cita ya está en un estado final.</p>') +
            '<div id="citaFormError" class="text-elara-accent small mt-2"></div>';

        modalBody.querySelectorAll(".js-cambiar-estado").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var nuevoEstado = boton.getAttribute("data-estado");
                var formData = new FormData();
                formData.append("id", evento.id);
                formData.append("estado", nuevoEstado);
                formData.append("__RequestVerificationToken", obtenerTokenGlobal());

                fetch(config.cambiarEstadoUrl, { method: "POST", body: formData })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            modal.hide();
                            mostrarToast("Estado actualizado a \"" + nuevoEstado + "\".", "success");
                            if (calendar) { calendar.refetchEvents(); }
                        } else {
                            document.getElementById("citaFormError").textContent = data.error || "No se pudo actualizar el estado.";
                        }
                    });
            });
        });

        modal.show();
    }
})();
