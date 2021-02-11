var listado = [];

$(document).ready(function () {
    console.log("Script tabla dinámica inicializado.")
});

function agregarFila(idTemplate, idTabla, contador) {
    try {
        
        //$('#' + idTemplate)
        //    .clone()                        // Clona elemento del DOM
        //    .attr('id', 'row' + (contador))    // Setea ID unico
        //    .appendTo($('#' + idTabla + ' tbody'))  // Agrega la fila
        //    .show();

        var clonar = $('#' + idTemplate).clone();
        //Copiar también valores en los SELECTS
        var selects = $('#' + idTemplate).find("select");
        $(selects).each(function (i) {
            var select = this;
            $(clonar).find("select").eq(i).val($(select).val());
        });

        clonar.attr('id', 'row' + (contador))    // Setea ID unico
            .appendTo($('#' + idTabla + ' tbody'))  // Agrega la fila
            .show();

        var fila = $('#row' + contador); // Captura la fila
        var celdaAcciones = $(fila).find("#accion"); // Captura la celda de las acciones
        celdaAcciones.html('<button type="button" id="eliminar' + contador + '" class="eliminarFila btn btn-danger"><i class="fa fa-trash-o" aria-hidden="true"></i></button>'); // Reemplaza la celda por el botón eliminar

        limpiarControles(idTemplate); // Limpiar Valores

        $('#eliminar' + contador).click(function (e) {
            e.preventDefault();
            var idFila = 'eliminar' + contador;
            eliminarFila(e.currentTarget)
            //if (idTabla == "tbl-Participantes")
            //    CalculoEstadisticasParticipantes(idTabla)

        });

        $(".datepicker").datepicker({
            autoclose: true,
            format: 'dd/mm/yyyy',
        });

        //$('.js-example-basic-single').select2();

        $(".informacion-equipo").click(function (e) {
            
            let elemento = $(e.currentTarget);

            var fila = elemento.closest("tr"); // fila
            var ddlPrefactura = fila.find("select"); // dropdownlist
            var valor = parseInt($(ddlPrefactura).val()); // valor seleccionado

            if (!valor) {
                toastr.error('Seleccione una opción.')
                return;
            }

            _GetCreate({ id: valor }, urlAccionInformacionEquipo);
            $('#contenido-modal').modal({
                'show': 'true',
                'backdrop': 'static',
                'keyboard': false
            });
            return;
        })


        //$('.chk-presente').change(function (e) {
        //    
        //    if ($(e).is(":checked")) {
        //        $(e).val("true");
        //    } else {
        //        $(e).val("false");
        //    }

        //    var elemento = $(e);
        //    var idTabla = $(e).closest('table').attr('id');
        //    CalculoEstadisticasParticipantes(idTabla, 'Se suspende.')
        //});



    }
    catch (error) {
        console.log(error);
    }
}

function limpiarControles(idTemplate) {
    $("#" + idTemplate).find("input[type=text],select").not(".idObjeto").val(""); // Inputs textbox
    $("#" + idTemplate + " input[type='checkbox']").attr('checked', false)
    $("#" + idTemplate + " input[type='checkbox']").val('false')

    $('#Institucion').val('').trigger('change')
    $('#Titulo').val('').trigger('change')
    $('#Cargo').val('').trigger('change')

    //$("select option:first-child").attr("selected", "selected");

    //$('#' + idTemplate + ' .chk-presente').iCheck('uncheck');
}

function eliminarFila(elemento) {
    
    try {
        var row = elemento.parentNode.parentNode;
        var IDElemento = $(row).find("td:first").html();
        $(row).remove();
    } catch (error) {
        console.log(error)
    }
}

function GetListadoTablaDinamica(tablaID, tieneFilaTemplate = true) {
    try {
        var tabla = $("#" + tablaID);

        listado = tabla.tableToJSON({
            extractor: function (cellIndex, $cell) {
                // get text from the span inside table cells;
                // if empty or non-existant, get the cell text
                //$('#Foto')[0].files[0]
                let tipo = $cell.find('input,select').attr('type'); 

                if (tipo == 'file')
                    return $cell.find('input')[0].files[0];

                return $cell.find('input,select').val() || $cell.text() /*|| $('input[type=checkbox]').val()*/
            }
        });
        //
        //Eliminando el primer elemento #template(fila vacía)


        if (listado.length > 0 && tieneFilaTemplate)
            listado = listado.slice(1)

        return listado;
    } catch (e) {
        console.log(e);
        return listado;
    }
}


function GetListadoFilesTablaDinamica(tablaID) {
    var archivos = [];
    try {
        var tabla = $("#" + tablaID);

        listado = tabla.tableToJSON({
            extractor: function (cellIndex, $cell) {
                // get text from the span inside table cells;
                // if empty or non-existant, get the cell text
                //$('#Foto')[0].files[0]
                let tipo = $cell.find('input,select').attr('type');
                if (tipo == 'file') {
                    
                    var reader = new FileReader();
                    reader.readAsDataURL($cell.find('input')[0].files[0]);
                    reader.onload = function () {
                        console.log(reader.result);
                        archivos.push(reader.result)
                    };

                    //getFile($cell.find('input')[0].files[0]).then((customJsonFile) => {
                    //    //customJsonFile is your newly constructed file.
                    //    archivos.push(customJsonFile.base64StringFile)
                    //});
                }

            }
        });
        //
        //Eliminando el primer elemento #template(fila vacía)
        if (listado.length > 0)
            listado = listado.slice(1)

        return archivos;
    } catch (e) {
        console.log(e);
        return archivos;
    } finally {
        return archivos;
    }
}

//take a single JavaScript File object
function getFile(file) {
    var reader = new FileReader();
    return new Promise((resolve, reject) => {
        reader.onerror = () => { reader.abort(); reject(new Error("Error parsing file")); }
        reader.onload = function () {

            //This will result in an array that will be recognized by C#.NET WebApi as a byte[]
            let bytes = Array.from(new Uint8Array(this.result));

            //if you want the base64encoded file you would use the below line:
            let base64StringFile = btoa(bytes.map((item) => String.fromCharCode(item)).join(""));

            //console.log("ejecutado")
            //archivosTest.push(base64StringFile);
            //Resolve the promise with your custom file structure
            resolve({
                bytes: bytes,
                base64StringFile: base64StringFile,
                fileName: file.name,
                fileType: file.type
            });
        }
        reader.readAsArrayBuffer(file);
    });
}


//Valida que todos los inputs sean obligatorios
function TablaDinamicaVacia(tablaID) {
    try {
        
        let elementoVacio = false;
        var tabla = $("#" + tablaID);

        listado = tabla.tableToJSON({
            extractor: function (cellIndex, $cell) {
                
                var numeroFila = $cell.closest("tr").index();
                let tipo = $cell.find('input,select').attr('type');

                if ($cell.find('input,select').val() == "" /*&& $cell.text() == ""*/ && numeroFila > 0 && tipo != 'file') {
                    console.log($cell.find('input'))
                    //console.log($cell.text());
                    elementoVacio = true;
                }

                //Para verificar los input de tipo archivo
                //if (tipo == 'file' && numeroFila > 0) {
                //    
                //    let archivo = $cell.find('input')[0].files[0];
                //    if (archivo.length == 0)
                //        elementoVacio = true;
                //}

                // get text from the span inside table cells;
                // if empty or non-existant, get the cell text
                //return $cell.find('input').val() || $cell.text()
            }
        });

        if (elementoVacio)
            return true;

        //Eliminando el primer elemento #template(fila vacía)
        if (listado.length > 0)
            listado = listado.slice(1)

        //Eliminando el primer elemento #template(fila vacía)
        if (listado.length > 0)
            return false;
        else
            return true;

    } catch (e) {
        console.log(e);
        return true;
    }
}


function SetListadoTablaDinamica(lista) {
    listado = lista;
}

function filaVacia(idTemplate) {
    var objetos = $("#" + idTemplate).find("input,select");
    let flag = false;
    for (var i = 0; i < objetos.length; i++) {
        let elemento = $(objetos[i]);
        let clase = elemento.attr('class');
        //if (!elemento.val() && clase !== 'chosen-search-input') // --> PARA COMPONENTE CON BUSQUEDA
        if (!elemento.val())
            flag = true;
    }
    return flag;
}

function DetallesPendientes(ids) {
    let flag = false;
    $.each(ids, function (index, value) {
        
        var objetos = $("#" + value).find("input,select");
        for (var i = 0; i < objetos.length; i++) {
            let elemento = $(objetos[i]);
            if (elemento.val() && !$(elemento[0]).hasClass('idObjeto') && !$(elemento[0]).hasClass('tipoFecha') && !$(elemento[0]).hasClass('chk-presente')) {
                flag = true;
                console.log("seccion: " + value)
                console.log(elemento)
            }
        }
    })
    return flag;
}

// Convierte un listado en una tabla HTML
function ListToTableHTML(listado, contenedorID, tablaID) {
    try {
        
        //var rows = [{ "firstName": "John", "last Name": "Doe", "age": "46" },{ "firstName": "James", "last Name": "Blanc", "age": "24" }];
        var html = '<table id="' + tablaID + '" class="table table-bordered table-hover">';
        html += '<tr>';
        for (var j in listado[0]) {
            html += '<th  data-override="' + j + '">' + j.match(/[A-Z][a-z]+|[0-9]+/g).join(" ") + '</th>';
        }
        html += '<th></th>'
        html += '</tr>';
        for (var i = 0; i < listado.length; i++) {
            html += '<tr>';
            for (var j in listado[i]) {
                html += '<td>' + listado[i][j] + '</td>';
            }
            html += '<td> <button type="button" title="Eliminar detalle." onclick="EliminarFilaTabla(this,listado)" class="eliminarFila btn btn-danger"><i class="fa fa-trash-o" aria-hidden="true"></i></button> </td>';
            html += '</tr>';
        }
        html += '</table>';

        document.getElementById(contenedorID).innerHTML = html;

        $('th:nth-child(1)').hide();
        $('td:nth-child(1)').hide();

    } catch (e) {
        console.log(e);
        document.getElementById(contenedorID).innerHTML = e;
    }
}

function EliminarFilaTabla(elemento) {
    
    var row = elemento.parentNode.parentNode;

    var IDElemento = $(row).find("td:first").html();

    //Para que se elimine correctamente la variable 'listado' tiene que ser declarada como global en el formulario.
    listado = listado.filter(item => item.IDContador != IDElemento);


    $(row).remove();



}

function removeItem(array, value) {
    var index = array.indexOf(value);
    if (index > -1) {
        array.splice(index, 1);
    }
    return array;
}
