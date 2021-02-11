//Recarga todos los grids del documento
function recargarGrids() {
    
    var enDocumento = document.querySelector('.mvc-grid');
    if (enDocumento !== null) {
        var grid = new MvcGrid(document.querySelector('.mvc-grid'));
        grid.query.set('search', this.value);
        //Do things
        [].forEach.call(document.getElementsByClassName('mvc-grid'), function (element) {
            
            new MvcGrid(element).reload();
        });
    }
}

// Recarga el grid por el parametro ID
function recargarGridByID(idGrid) {
    
    var grid = new MvcGrid(document.querySelector("#" + idGrid));
    grid.query["parameters"] = [];
    grid.query.set('search', '');
    grid.reload();
    //Do things
    //[].forEach.call(document.getElementsByClassName('mvc-grid'), function (element) {
    //    
    //    new MvcGrid(element).reload();
    //});
}

// Recarga el grid con varios parametros de catalogo
function recargarGridByCatalogos(idGrid, tipo, subcatalogo, filtro) {
    
    var grid = new MvcGrid(document.querySelector("#" + idGrid));
    grid.query["parameters"] = [];
    grid.query.set('search', '');
    grid.query.set('tipo', tipo);
    grid.query.set('subcatalogo', subcatalogo);
    grid.query.set('filtro', filtro);
    grid.reload();
    //Do things
    //[].forEach.call(document.getElementsByClassName('mvc-grid'), function (element) {
    //    
    //    new MvcGrid(element).reload();
    //});
}


//Búsqueda general en Grid
//function busquedaGrid(gridID) {
//    
//    var filtro = $('#' + gridID).find('input');//document.getElementById('GridSearchGeneral');
//    if (filtro !== null) {
//        
//        var grid = new MvcGrid(document.querySelector('#' + gridID));
//        grid.query.set('search', $(filtro).val());
//        grid.reload();
//    } else {
//        toastr.error("Ocurrió un error al cargar los datos.")
//    }

//}

//Búsqueda general en Grid
function busquedaGrid(ID) {
    
    grid = document.getElementById('GridSearchGeneral');
    if (grid !== null) {
        document.getElementById('GridSearchGeneral').addEventListener('input', function () {
            
            var grid = new MvcGrid(document.querySelector('#' + ID));
            grid.query.set('search', this.value);
            grid.reload();
        });
    } else {
        toastr.error("Ocurrió un error al cargar los datos.")
    }
}

//Búsqueda general en Grid
function busquedaGridByCedula(ID) {
    
    grid = document.getElementById('GridSearchIdentificacion');
    if (grid !== null) {
        document.getElementById('GridSearchIdentificacion').addEventListener('input', function () {
            
            var grid = new MvcGrid(document.querySelector('#' + ID));
            grid.query.set('cedula', this.value);
            grid.query.delete('placa');
            grid.reload();
        });
    } else {
        toastr.error("Ocurrió un error al cargar los datos.")
    }
}

//Búsqueda general en Grid
function busquedaGridByPlaca(ID) {
    
    grid = document.getElementById('GridSearchPlaca');
    if (grid !== null) {
        document.getElementById('GridSearchPlaca').addEventListener('input', function () {
            
            var grid = new MvcGrid(document.querySelector('#' + ID));
            grid.query.set('placa', this.value);
            grid.query.delete('cedula');
            grid.reload();
        });
    } else {
        toastr.error("Ocurrió un error al cargar los datos.")
    }
}

function reporteGridPDF(urlAccion) {
    
    $.ajax({
        type: "POST",
        url: urlAccion,
        content: "application/json; charset=utf-8",
        dataType: "json",
        //data: JSON.stringify(data),
        success: function (data) {
            
            var atributos = Object.keys(data[0])
            var propiedades = [];

            for (var i = 0; i < atributos.length; i++) {
                propiedades.push({
                    'field': atributos[i],
                    'displayName': atributos[i],
                })
            }

            //JSON.parse(resultado)
            //console.log(resultado.Data)
            printJS({
                printable: data,
                properties: propiedades,
                type: 'json'
            })
        },
        error: function (xhr, textStatus, errorThrown) {
            
            toastr.error("Ocurrió un error.")
        }
    });
}

function LimpiarFiltroBusquedaGeneralGrid() {
    $("#GridSearchGeneral").val('');
    let grid = new MvcGrid(document.querySelector('.mvc-grid'));
    grid.query.set('search', '');
}

function CantidadElementosGrid() {
    var contador = 0;
    var item = $(".mvc-grid").find("tbody").children();

    $.each(item, function (index, value) {
        contador++;
    });

    return contador;
}