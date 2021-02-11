// Necesita que la clase del div que agrupa el form-group tenga el mismo nombre que el elemento a ocultar para que se oculte la sección completa (label e input)
function mostrarElementosFormulario() {
    $('form input, form select').each(
        function (index, valor) {
            //
            var seccion = valor.name;

            if (seccion.length > 0)
                $("." + seccion).show();
            //console.log('Type: ' + input.attr('type') + 'Name: ' + input.attr('name') + 'Value: ' + input.val());
        }
    );
}

// Necesita que la clase del div que agrupa el form-group tenga el mismo nombre que el elemento a ocultar para que se oculte la sección completa (label e input)
function ocultarListadoElementos(campos) {
    $.each(campos, function (index, value) {
        //
        if (value.length > 0)
            $("." + value).hide();
    });
}

function validarCamposRequeridos(formulario) {
    var flag = true;
    $('#' + formulario + ' .campo-requerido').each(function (index, value) {
        
        var elemento = $(this).val();
        var idElemento = this.id;
        if ((elemento == "" || elemento === null || elemento === undefined) && $(this).is(":visible")) {
            flag = false;
            console.log(idElemento)
        }
    });
    return flag;
}

//Campos visibles u ocultos
function validarCamposRequeridosFormularioCompleto(formulario) {
    var flag = true;
    $('#' + formulario + ' .campo-requerido').each(function (index, value) {
        var elemento = $(this).val();
        var idElemento = this.id;
        if ((elemento == "" || elemento === null || elemento === undefined) && idElemento.length > 0) {
            flag = false;
            console.log(idElemento)
        }
    });
    return flag;
}

$.fn.serializeObject = function () {
    //
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        //
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

//String.prototype.format = function () {
//    var s = this,
//        i = arguments.length;

//    while (i--) {
//        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
//    }
//    return s;
//};

// First, checks if it isn't implemented yet.
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
                ? args[number]
                : match
                ;
        });
    };
}



// De formato 00.00 a Format 00.00,00
function formatearNumeroMilesDecimales(numero) {
    var valorPorDefecto = 0;
    try {
        var conversion = new Intl.NumberFormat('de-DE',
            {
                currency: 'EUR',
                //maximumSignificantDigits: 3,
                //minimumSignificantDigits: 3,
                minimumFractionDigits: 6,
                //maximumFractionDigits: 6,
            }).format(numero);
        //conversion = conversion  0 ? conversion : "00.000,000000"
        return conversion;
    }
    catch (e) {
        console.log(e)
        // sentencias para manejar cualquier excepción
        return valorPorDefecto;
    }
}

function formatearNumeroFormatoComun(numero) {
    numero = numero === undefined || numero === null ? "0" : numero;
    var formatoNormal = numero.replace(/\./g, "");
    return formatoNormal;
}


function habilitarBotonCargaMasiva(retardo) {
    

    setTimeout(function () {
        appendBotonCargaMasiva()
    }, retardo);
}

function appendBotonCargaMasiva() {
    var seccionElementoVerificacion = $("#seccion-botones-funciones").find("#cargar-data");

    if (seccionElementoVerificacion.length === 0)
        $("#seccion-botones-funciones").append("<button title='Cargar información masiva.' style='background-color:rgb(254, 80, 0);border-color: rgb(254, 80, 0);' class='btn btn-danger' data-toggle='modal'  data-backdrop='static' data-target='#cargas-masivas-modal'  id='cargar-data'><span class='glyphicon glyphicon-open'></span> </button>");
}

function LimpiarFormularioCargasMasivas(idGridCargasMasivas, idFileUpload, classLabel) {
    $("#" + idFileUpload).val(null);
    $("#" + idGridCargasMasivas + "tbody").empty();
    $("#" + idGridCargasMasivas).hide();
    $('.' + classLabel).html("<i class='fa fa-cloud-upload'></i>Seleccionar Archivo</label>");
}

function ControlesCargaEnEspera(idSeccionContenidoCargasMasivas, idBotonProcesarCarga, idInputFile) {
    $('#' + idSeccionContenidoCargasMasivas).css('cursor', 'wait');
    $('.custom-file-upload').css('cursor', 'wait');
    $("#" + idBotonProcesarCarga).attr('disabled', 'disabled');
    $("#" + idInputFile).attr('disabled', 'disabled');
    $(".close").hide();
}

function ControlesCargaFinalizada(idSeccionContenidoCargasMasivas, idBotonProcesarCarga, idInputFile) {
    $('#' + idSeccionContenidoCargasMasivas).css('cursor', 'default');
    $('.custom-file-upload').css('cursor', 'pointer');
    $("#" + idBotonProcesarCarga).removeAttr('disabled');
    $("#" + idInputFile).removeAttr('disabled');
    $(".close").show();
}


function DeshabilitarAccionNuevo() {
    
    $("#nuevo").delay(100).hide();
}

function DeshabilitarAccionExportarPDF() {
    $("#ExportarGridPDF").delay(100).hide();
}

function DeshabilitarAccionExportarCSV() {
    $("#ExportarGridCSV").delay(100).hide();
}


function countProps(obj) {
    var count = 0;
    for (k in obj) {
        if (obj.hasOwnProperty(k)) {
            count++;
        }
    }
    return count;
};

function CompararObjetos(v1, v2) {

    if (typeof (v1) !== typeof (v2)) {
        return false;
    }

    if (typeof (v1) === "function") {
        return v1.toString() === v2.toString();
    }

    if (v1 instanceof Object && v2 instanceof Object) {
        if (countProps(v1) !== countProps(v2)) {
            return false;
        }
        var r = true;
        for (k in v1) {
            r = CompararObjetos(v1[k], v2[k]);
            if (!r) {
                return false;
            }
        }
        return true;
    } else {
        return v1 === v2;
    }
}

function zoomin(idElemento) {
    
    var myImg = document.getElementById(idElemento);
    var currWidth = myImg.clientWidth;
    if (currWidth == 2500) return false;
    else {
        myImg.style.width = (currWidth + 100) + "px";
    }
}
function zoomout(idElemento) {
    
    var myImg = document.getElementById(idElemento);
    var currWidth = myImg.clientWidth;
    if (currWidth == 100) return false;
    else {
        myImg.style.width = (currWidth - 100) + "px";
    }
}


function AgregarElementoArreglo(array, elemento) {
    
    const index = array.indexOf(elemento);
    if (index === -1) {
        array.push(elemento)
    }
    return array;
}

function AgregarMultiplesElementosArreglo(array, elementos) {
    
    for (var i = 0; i < elementos.length; i++) {
        const index = array.indexOf(elementos[i]);
        if (index === -1) {
            array.push(elementos[i])
        }
    }
    return array;
}

function EliminarElementoArreglo(array, elemento) {
    
    array = array.filter(item => item != elemento)
    return array;
}

function EliminarMultiplesElementosArreglo(array, elementos) {
    
    for (var i = 0; i < elementos.length; i++) {
        array = array.filter(item => item != elementos[i])
    }
    return array;
}

function GetElementosSeleccionadosGrid(clsChecks) {
    let ids = [];
    $.each($("." + clsChecks), function (index, value) {
        //elemento = $(this).attr('id').split("-")[1];
        elemento = parseInt($(this).attr('id'));
        if ($(this).prop("checked") == true)
            ids.push(elemento);
    })
    return ids;
}

// Funciones para cuando se ejecutan acciones hacia controladores.
function ProcessWait(idFormulario) {
    $('#' + idFormulario + ' *').prop('disabled', true);
    $(".content").addClass('wait-loading-form');
}

function ProcessReady() {
    $(".content").addClass('wait-loading-form');
}

function CargarValoresSelect2(listado, IDElementoSelect2) {
    
    $.each(listado, function (index, value) {
        
        // Create a DOM Option and pre-select by default
        var newOption = new Option(value.Text, value.Value, true, true);
        // Append it to the select
        $('#' + IDElementoSelect2).append(newOption).trigger('change');
    })
}

//Validar que todas las propiedades de un arreglo tengan valor.
function HasEmptyKeysArray(array){
    let flag = false;
    for (var i = 0; i < array.length; i++) {
        let objeto = array[i];
        for (var key in objeto) {
            if (objeto[key] === "" || objeto[key] === null || !!objeto[key]) { // || objeto[key] === 0
                console.log(key + " está vacio. | : " + objeto[key]);
                flag = true;
            }
        }
    }
    return flag;
}

function soloLetras(e) {
    key = e.keyCode || e.which;
    tecla = String.fromCharCode(key).toLowerCase();
    letras = " áéíóúabcdefghijklmnñopqrstuvwxyz";
    especiales = "8-37-39-46";

    tecla_especial = false
    for (var i in especiales) {
        if (key == especiales[i]) {
            tecla_especial = true;
            break;
        }
    }

    if (letras.indexOf(tecla) == -1 && !tecla_especial) {
        return false;
    }
}