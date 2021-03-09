function ConfirmarEliminacion_(id, urlAccion) {
    
    swal({
        title: "Estás seguro de eliminar?",
        text: "Podrían exister registros dependientes!",
        icon: "warning",
        buttons: ["Cancelar", "OK"],
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            _eliminar(id, urlAccion)
        }
    });
}

function ConfirmarSeleccionEmpresa_(parametros, urlAccion) {
    swal({
        title: "¿Estás seguro de seleccionar esta empresa?",
        text: "Se creará un caso para el registro seleccionada. El cliente podría tener varias empresas!",
        icon: "info",
        buttons: ["Cancelar", "OK"],
        dangerMode: false,
    }).then((willDelete) => {
        if (willDelete) {
            _seleccionarEmpresa(parametros, urlAccion)
        }
    });
}

function _seleccionarEmpresa(parametros, urlAccion) {
    
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: parametros,
        dataType: 'json',
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                swal(result.Respuesta, { icon: "success" }).then((value) => {
                    location.reload();
                });
            }
            else {
                swal("Revisar Información!", result.Respuesta, "info").then((value) => {
                    location.reload();
                });
            }
            //location.reload();
        },
        error: function (result) {
            
            console.log(result)
            swal("No se pudo realizar la acción!", "Consulte con el administrador o verifique los permisos de su cuenta.", "info").then((value) => {
                location.reload();
            });
        }
    });
}

function ConfirmarAnularPrefactura_(id, urlAccion) {
    
    swal({
        title: "Estás seguro de anular el Presupuesto?",
        text: "",
        icon: "warning",
        buttons: ["Cancelar", "OK"],
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            _eliminar(id, urlAccion)
        }
    });
}

function ConfirmarPresupuesto_(id, urlAccion) {
    
    swal({
        title: "¿Desea Generar el Presupuesto?", 
        icon: "warning",
        buttons: ["NO", "SI"],
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            _presupuesto(id, urlAccion)
        }
    });
}

function ConfirmarFactura_(id, urlAccion) {
    
    swal({
        title: "¿Desea Generar la Factura?",
        icon: "warning",
        buttons: ["NO", "SI"],
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            _presupuesto(id, urlAccion)
        }
    });
}

function ConfirmarCambioEstado(id, urlAccion, estado) {
    
    var titulo;
    if (estado === "Activo") {
        titulo = "¿Estás seguro de inactivar el registro?";
    }

    if (estado === "Inactivo") {
        titulo = "¿Estás seguro de activar el registro?";
    }
    
    swal({
        title: titulo,
        text: "Podrían exister registros dependientes!",
        icon: "warning",
        buttons: ["Cancelar", "OK"],
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            _eliminar(id, urlAccion)
        }
    });
}

function _eliminar(id, urlAccion) {
    
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: { id: id },
        dataType: 'json',
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                swal(result.Respuesta, { icon: "success" }).then((value) => {
                    location.reload();
                });
            }
            else {
                swal("Revisar Información!", result.Respuesta, "info").then((value) => {
                    location.reload();
                });
            }
            //location.reload();
        },
        error: function (result) {
            
            console.log(result)
            swal("No se pudo realizar la acción!", "Consulte con el administrador o verifique los permisos de su cuenta.", "info").then((value) => {
                location.reload();
            });
        }
    });
}

function _presupuesto(id, urlAccion) {
    
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: { id: id },
        dataType: 'json',
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                swal(result.Respuesta, { icon: "success" }).then((value) => {
                    location.reload();
                });
            }
            else {
                swal("Revisar Información!", result.Respuesta, "info").then((value) => {
                    location.reload();
                });
            }
            //location.reload();
        },
        error: function (result) {
            
            console.log(result)
            swal("No se pudo realizar la acción!", "Consulte con el administrador o verifique los permisos de su cuenta.", "info").then((value) => {
                location.reload();
            });
        }
    });
} 

function _facturar(id, urlAccion) {
    
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: { id: id },
        dataType: 'json',
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                swal(result.Respuesta, { icon: "success" }).then((value) => {
                    location.reload();
                });
            }
            else {
                swal("Revisar Información!", result.Respuesta, "info").then((value) => {
                    location.reload();
                });
            }
            //location.reload();
        },
        error: function (result) {
            
            console.log(result)
            swal("No se pudo realizar la acción!", "Consulte con el administrador o verifique los permisos de su cuenta.", "info").then((value) => {
                location.reload();
            });
        }
    });
}

function _GuardarGeneral(data, urlAccion, urlListado) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            //$("#cerrar-modal").trigger("click");

            if (result.Estado) {
                toastr.success(result.Respuesta)
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }

            recargarGrids()

            setTimeout(function () {
                window.location.replace(urlListado);
            }, 500);

        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });

}

function _GuardarSinVolverListado(data, urlAccion) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
            recargarGrids()
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarGenerico(data, urlAccion) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
            }
            else {
                $("#seccion-cargando-loading").hide();
                toastr.error(result.Respuesta)
                return;
            }

            setTimeout(function () {
                location.reload();
            }, 500);
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarGenericoLoading(data, urlAccion, urlListado) {
    
    $("#preloader").show();
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                $("#preloader").hide();
            }
            else {
                toastr.error(result.Respuesta)
                $("#preloader").hide();
                return;
            }
            //recargarGrids()
             
            setTimeout(function () {
                window.location.replace(urlListado);
            }, 150);

        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarPrefacturasLoading(data, urlAccion, urlListado) {
    
    $("#preloader").show();
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                $("#preloader").hide(); 
            }
            else {
                toastr.error(result.Respuesta)
                $("#preloader").hide(); 
                return;
            } 

            setTimeout(function () {
                window.location.replace(urlListado);
            }, 350);
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function DescargarArchivosActas(_data, urlAccion, urlAccionDescarga) {
    
    $("#preloader").show();
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: _data,
        dataType: 'json',
        success: function (result) {
            
            var archivos = result.PathsArchivos;
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                var contador = 1000;
                for (var i = 0; i < archivos.length; i++) {
                    let url = urlAccionDescarga + "?path=" + archivos[i];
                    setTimeout(function () {
                        location.href = url;
                    }, contador);
                    contador += 1000;
                }
            }
            else {
                setTimeout(function () {
                    toastr.error(result.Respuesta)
                    $("#preloader").hide();
                }, 500);

                return;
            }
            $("#preloader").hide();
            //setTimeout(function () {
            //    location.reload();
            //}, 500);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#preloader").hide();
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function DescargarArchivos(_data, urlAccion, urlAccionDescarga) {
    
    $("#preloader").show();
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: _data,
        dataType: 'json',
        success: function (result) {
            
            var archivos = result.PathsArchivos;
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                var contador = 1000;

                let url = urlAccionDescarga + "?path=" + archivos;
                setTimeout(function () {
                    location.href = url;
                }, contador);
                contador += 1000;

            }
            else {
                setTimeout(function () {
                    toastr.error(result.Respuesta)
                    $("#preloader").hide();
                }, 500);

                return;
            }
            $("#preloader").hide();
            //setTimeout(function () {
            //    location.reload();
            //}, 500);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#preloader").hide();
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}


    function _Guardar2(data, urlAccion, urlListado) {
        
        $.ajax({
            contentType: false, // Not to set any content header
            //dataType: 'json',
            type: 'POST',
            url: urlAccion,
            data: data,
            processData: false, // Not to process data
            success: function (result) {
                
                result = result.Resultado;

                if (result.Estado) {

                    $("#cerrar-modal").trigger("click");
                    toastr.success(result.Respuesta);

                    setTimeout(function () {
                        if (urlListado === null || urlListado === undefined) {
                            return;
                        }
                        else {
                            window.location.replace(urlListado);
                        }
                    }, 500);
                }
                else {
                    toastr.error(result.Respuesta)
                    return;
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                
                toastr.error("Codigo: " + xhr.status)
                toastr.error("Tipo: " + textStatus)
                toastr.error("Detalle: " + errorThrown.message)
            }
        });
    }


function _Guardar(data, urlAccion, urlListado) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        //contentType: false,
        processData: false,
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {

                $("#cerrar-modal").trigger("click");
                toastr.success(result.Respuesta);

                setTimeout(function () {
                    if (urlListado === null || urlListado === undefined) {
                        return;
                    }
                    else {
                        window.location.replace(urlListado);
                    }
                }, 500);
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarCotizador(data, urlAccion, urlListado, cotizacion = false, urlCotizador = '') {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {

                $("#cerrar-modal").trigger("click");
                toastr.success(result.Respuesta);

                //Solo para la cotización - La respuesta que devuelve el método cotizador trae el ID creado del Código de Cotización, que se va a enviar como parámetro
                var idCodigoCotizacion = result.CotizacionID;

                // Parametro solo para cotizador
                var idCotizador = result.CotizadorID;

                if (idCotizador > 0) {
                    var cualquiera = urlCotizador.format(idCotizador);

                    window.location.replace(cualquiera);

                    return;
                }

                $("#cotizadorID").val(idCotizador);
                $("#generarCotizacion").show();

                //recargarGrids();
                if (cotizacion || cotizacion === undefined || cotizacion === null) {
                    swal({
                        title: "¿Desea crear una Cotización?",
                        //text: "Podrían exister registros dependientes!",
                        icon: "info",
                        buttons: ["NO", "SI"],
                        dangerMode: false,
                        //backdrop: false,
                        closeOnClickOutside: false,
                        closeOnEsc: false,
                    })
                        .then((willDelete) => {
                            if (willDelete) {
                                //console.log('Redireccion hacia el cotizador: ' + urlCotizador)
                                //console.log(urlCotizador.format(idCodigoCotizacion));
                                window.location.replace(urlCotizador.format(idCodigoCotizacion));
                            } else {
                                window.location.replace(urlListado);
                                //if (urlListado === null || urlListado === undefined)
                                //    window.location.replace(urlListado);
                                //else
                                //    return;
                            }
                        });
                } else {
                    setTimeout(function () {
                        if (urlListado === null || urlListado === undefined) {
                            return;
                        }
                        else {
                            window.location.replace(urlListado);
                        }

                    }, 500);
                }
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarModal(data, urlAccion) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {

                $("#btn-cerrar-modal").trigger("click");
                toastr.success(result.Respuesta);
                location.reload();
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _SaveData(data, urlAccion) {
    
    return $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                $("#btn-cerrar-modal").trigger("click");
                toastr.success(result.Respuesta);
                return;
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function _GuardarModalStatusCodigoCotizacion(data, urlAccion) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: function (result) {
            
            result = result.Resultado;
            if (result.Estado) {
                //$("#cerrar-modal").trigger("click");
                $('#contenido-modal').modal('hide');
                toastr.success(result.Respuesta);

                setTimeout(function () {
                    location.reload();
                }, 500);
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

// Renderizar formulario de Detalles
function _GetDetalles(id, urlAccion) {
    mostrarIconoCargando();
    $.ajax({
        url: urlAccion,
        type: "GET",
        dataType: "html",
        data: { id: id },
        success: function (data) {
            setTimeout(function () {
                $("#main-Modal").html(data);
                // Limpiando el loading
                ocultarIconoCargando();
            }, 500);
        },
        error: function (result) {
            swal("Error!", result.Respuesta, "error").then((value) => {
                location.reload();
            });
        }
    });
}

// Renderizar formulario de Creación
function _GetCreate(parametros, urlAccion, modalID = 'contenido-modal') {
    
    mostrarIconoCargando(modalID);
    $.ajax({
        url: urlAccion,
        type: "GET",
        dataType: "html",
        data: parametros,
        cache: false,
        success: function (data) {
            
            setTimeout(function () {
                let mainModal = $("#" + modalID).find('#main-Modal');
                $(mainModal).html(data);
                //$("#main-Modal").html(data);
                // Limpiando el loading
                ocultarIconoCargando(modalID);
            }, 500);

            modalReady = true;
        },
        error: function (result) {
            
            console.log(result)
            swal("¡Error!", result.Respuesta, "error").then((value) => {
                //location.reload();
                console.log(result)
            });
        }
    });
}

// Renderizar formulario de Creación
function _PostCreate(parametros, urlAccion, modalID = 'contenido-modal') {
    
    mostrarIconoCargando(modalID);
    $.ajax({
        url: urlAccion,
        type: "POST",
        dataType: "html",
        data: parametros,
        success: function (data) {
            
            setTimeout(function () {
                let mainModal = $("#" + modalID).find('#main-Modal');
                $(mainModal).html(data);
                // Limpiando el loading
                ocultarIconoCargando(modalID);
            }, 500);
        },
        error: function (result) {
            
            console.log(result)
        }
    });
}

function CargaMasivaData(archivo, urlAccion, gridErroresID) {

    var fileUpload = archivo;
    var files = fileUpload.files;
    // Create FormData object
    var fileData = new FormData();
    // Looping over all files and add it to FormData object
    for (var i = 0; i < files.length; i++) {
        fileData.append(files[i].name, files[i]);
    }
    $.ajax({
        url: urlAccion,
        type: "POST",
        contentType: false, // Not to set any content header
        processData: false, // Not to process data
        data: fileData,
        async: false,
        success: function (result) {
            
            LoadProgressBar(result, gridErroresID);
            //$('#seccion-cargas-masivas-contenido').css('cursor', 'context-menu');
            //$("#procesar-carga").attr('disabled', 'disabled');
        },
        error: function (xhr, textStatus, errorThrown) {
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        },
    });
}

function AdjuntarArchivo(archivo, urlAccion, data_form = null) {
    
    var fileUpload = archivo;
    var fileData = new FormData();

    if (fileUpload) {
        let files = fileUpload.files;
        // Create FormData object
        
        // Looping over all files and add it to FormData object
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }
    }


    
    // Permite enviar otros objetos en la peticion
    if (data_form !== null) {
        for (var nombre in data_form) {
            
            fileData.append(nombre, JSON.stringify({ [nombre]: data_form[nombre] }));
        }
    }

    return $.ajax({
        url: urlAccion,
        type: "POST",
        contentType: false, // Not to set any content header
        processData: false, // Not to process data
        data: fileData,
        async: false,
    });
}
 
function EliminarArchivoAdjunto(path, urlAccion, seccionArchivosID, componenteArchivosID) {
    
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: { pathArchivo: path },
        dataType: 'json',
        success: function (result) {
            
            var resultado = result.Resultado;

            if (resultado.Estado) {
                toastr.success(resultado.Respuesta)
                $("#" + seccionArchivosID).show();
                $('#' + componenteArchivosID).tree('reload');
            }
            else {
                toastr.error(resultado.Respuesta)
                $("#" + seccionArchivosID).show();
                $('#' + componenteArchivosID).tree('reload');
                return;
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        },
    });
}

function LoadProgressBar(result, gridErroresID) {
    var progressbar = $("#progressbar-5");
    var progressLabel = $(".progress-label");
    progressbar.show();
    $("#progressbar-5").progressbar({
        //value: false,  
        change: function () {
            progressLabel.text(
                progressbar.progressbar("value") + "%");  // Showing the progress increment value in progress bar  
        },
        complete: function () {
            progressLabel.text("Carga Completada!");
            progressbar.progressbar("value", 0);  //Reinitialize the progress bar value 0  
            progressLabel.text("");
            progressbar.hide(); //Hiding the progress bar  

            var resultado = result.Resultado;
            var errores = result.Errores;
            var nombreArchivo = result.Archivo;

            if (resultado.Estado) {
                toastr.success(resultado.Respuesta)
                ControlesCargaFinalizada("seccion-cargas-masivas-contenido", "procesar-carga", "file-upload");
                LimpiarFormularioCargasMasivas("grid-CargaMasiva", "file-upload", "custom-file-upload");
            }
            else {
                toastr.error(resultado.Respuesta)

                $("#" + gridErroresID).find(".mvc-grid-empty-row").remove(); // Vaciar Body Grid Errores
                $("#" + gridErroresID).find('tbody').empty();

                for (var i = 0; i < errores.length; i++) {
                    let row = "<tr>" + "<td>" + errores[i].Fila + "</td>" + "<td>" + errores[i].Columna + "</td>" + "<td>" + errores[i].Valor + "</td>" + "<td>" + errores[i].Error + "</td>" + "</tr>"
                    $("#" + gridErroresID).find('tbody').append(row);
                }
                $("#" + gridErroresID).show();
                ControlesCargaFinalizada("seccion-cargas-masivas-contenido", "procesar-carga", "file-upload");
                return;
            }

            //var markup = "<tr><td>" + result + "</td><td><a href='#' onclick='DeleteFile(\"" + result + "\")'><span class='glyphicon glyphicon-remove red'></span></a></td></tr>"; // Binding the file name  
            //$("#ListofFiles tbody").append(markup);
            //$('#Files').val('');
            //$('#FileBrowse').find("*").prop("disabled", false);
        }
    });
    function progress() {
        var val = progressbar.progressbar("value") || 0;
        progressbar.progressbar("value", val + 1);
        if (val < 99) {
            setTimeout(progress, 25);
        }
    }
    setTimeout(progress, 100);
}

function mostrarIconoCargando(modalID) {
    
    var mainModal = $("#" + modalID).find('#main-Modal');
    $(mainModal).html("")

    var zonaCargandoContenido = $("#" + modalID).find('#zonaCargandoContenido');
    $(zonaCargandoContenido).html(imgBuildLoading)
}

function ocultarIconoCargando(modalID) {
    
    var zonaCargandoContenido = $("#" + modalID).find('#zonaCargandoContenido');
    $(zonaCargandoContenido).html("")
    //$('#zonaCargandoContenido').html("")
}

function _GuardarAdjuntarArchivo(archivo, urlAccion, urlListado, data_form, formato, soporte, archivoHtml) {
    var fileUpload = archivo;
    var files = fileUpload.files;
    // Create FormData object
    var fileData = new FormData();
    // Looping over all files and add it to FormData object
    for (var i = 0; i < files.length; i++) {
        fileData.append(files[i].name, files[i]);
    }

    var fileUpload1 = formato;
    var files1 = fileUpload1.files; 
    for (var i = 0; i < files1.length; i++) {
        fileData.append(files1[i].name, files1[i]);
    }

    var fileUpload2 = soporte;
    var files2 = fileUpload2.files; 
    for (var i = 0; i < files2.length; i++) {
        fileData.append(files2[i].name, files2[i]);
    }

    var fileUpload3 = archivoHtml;
    var files3 = fileUpload3.files; 
    for (var i = 0; i < files3.length; i++) {
        fileData.append(files3[i].name, files3[i]);
    }

    

    fileData.append('solicitud', JSON.stringify(data_form.solicitud));
    fileData.append('codigoUsuario', data_form.codigoUsuario);
    fileData.append('urlExterno', JSON.stringify(data_form.urlExterno));
    fileData.append('camposPersonalizados', JSON.stringify(data_form.camposPersonalizados));
    fileData.append('listadoUrlSoporte', JSON.stringify(data_form.listadoUrlSoporte));

    
    $.ajax({
        url: urlAccion,
        type: "POST",
        contentType: false, // Not to set any content header
        processData: false, // Not to process data
        data: fileData,
        async: false,
        success: function (result) {
            
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
            }
            else {
                toastr.error(result.Respuesta)
                return;
            }

            setTimeout(function () {
                window.location.replace(urlListado);
            }, 500);

        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function DescargarArchivosFicha(_data, urlAccion, urlAccionDescarga) {
    
    $("#preloader").show();
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: _data,
        dataType: 'json',
        success: function (result) {
            
            var archivos = result.PathsArchivos;
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                var contador = 1000;
                for (var i = 0; i < archivos.length; i++) {
                    let url = urlAccionDescarga + "?path=" + archivos[i];
                    setTimeout(function () {
                        location.href = url;
                    }, contador);
                    contador += 1000;
                }
            }
            else {
                setTimeout(function () {
                    toastr.error(result.Respuesta)
                    $("#preloader").hide();
                }, 500);

                return;
            }
            $("#preloader").hide();
            //setTimeout(function () {
            //    location.reload();
            //}, 500);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#preloader").hide();
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}

function DescargarArchivosGeneral(_data, urlAccion, urlAccionDescarga) {
    
    $("#preloader").show();
    $.ajax({
        type: 'POST',
        url: urlAccion,
        data: _data,
        dataType: 'json',
        success: function (result) {
            
            var archivos = result.PathsArchivos;
            result = result.Resultado;

            if (result.Estado) {
                toastr.success(result.Respuesta)
                var contador = 1000;
                for (var i = 0; i < archivos.length; i++) {
                    let url = urlAccionDescarga + "?path=" + archivos[i];
                    setTimeout(function () {
                        location.href = url;
                    }, contador);
                    contador += 1000;
                }
            }
            else {
                setTimeout(function () {
                    toastr.error(result.Respuesta)
                    $("#preloader").hide();
                }, 500);

                return;
            }
            $("#preloader").hide();
            //setTimeout(function () {
            //    location.reload();
            //}, 500);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#preloader").hide();
            
            toastr.error("Codigo: " + xhr.status)
            toastr.error("Tipo: " + textStatus)
            toastr.error("Detalle: " + errorThrown.message)
        }
    });
}
 
function _GuardarPermisos(data, urlAccion, sucess, error) {
    
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: urlAccion,
        data: data,
        success: sucess,
        error: error
    });
}