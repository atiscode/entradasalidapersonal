$(document).ready(function () {
    //$(".animsition").animsition({
    //    inClass: 'fade-in',
    //    outClass: 'fade-out',
    //    inDuration: 1500,
    //    outDuration: 800,
    //    linkElement: '.animsition-link',
    //    // e.g. linkElement: 'a:not([target="_blank"]):not([href^="#"])'
    //    loading: true,
    //    loadingParentElement: 'body', //animsition wrapper element
    //    loadingClass: 'animsition-loading',
    //    loadingInner: '', // e.g '<img src="loading.svg" />'
    //    timeout: false,
    //    timeoutCountdown: 5000,
    //    onLoadEvent: true,
    //    browser: ['animation-duration', '-webkit-animation-duration'],
    //    // "browser" option allows you to disable the "animsition" in case the css property in the array is not supported by your browser.
    //    // The default setting is to disable the "animsition" in a browser that does not support "animation-duration".
    //    overlay: false,
    //    overlayClass: 'animsition-overlay-slide',
    //    overlayParentElement: 'body',
    //    transition: function (url) { window.location.href = url; }
    //});

    //$(document).ajaxComplete(function () {
    //    Pace.restart();
    //});

    //$(document).ajaxStart(function () {
    //    Pace.restart();
    //});

    debugger

    // Validando que existen grids en el documento
    var grids = document.getElementsByClassName('mvc-grid');
    if (grids.length > 0) {
        // Evento cuando se comienza a cargar el grid
        document.addEventListener('reloadstart', function (e) {
            console.log('grid reloadstart: ', e.detail.grid);
            //$('<div>', { id: 'overlay' }).appendTo('body');
        });

        // Para acciones de click sobre la fila del Grid
        document.addEventListener('rowclick', function (e) {
            debugger
            if (e.target.dataset.id != null) {
                console.log(e.target.dataset.id);
            }
        });

        // Evento cuando finaliza la carga del grid
        document.addEventListener('reloadend', function (e) {
            debugger
            var elementos = CantidadElementosGrid();
                
            //if (elementos > 1)
            //    $('#GridSearchGeneral').prop("disabled", false);
            //else 
            //    $('#GridSearchGeneral').prop("disabled", true);

            //console.log(elementos)
            console.log('grid reloadend: ', e.detail.grid);
            //console.log(elementosSeleccionadosGrid)

            debugger
            $("#cargando-grid").removeClass("preloader-grid");
            $("#cargando-grid").remove();
            var parametrosGrid = e.detail.grid.query["parameters"][0]; // Parametros
            var ParámetrosFiltros = e.detail.grid.query["parameters"];

            for (var i = 0; i < ParámetrosFiltros.length; i++) {
                //Solamente si es para el filtro de búsqueda
                var parametro = ParámetrosFiltros[i];
                var flagContieneBusqueda = parametro.indexOf("search");
                var flagContieneIdentificacion = parametro.indexOf("cedula");

                var flagContienePlaca = parametro.indexOf("placa");

                if (flagContieneBusqueda !== -1) {
                    var arrayParametros = parametro.split("="); // Separacion
                    var textoBusqueda = arrayParametros[1];
                    textoBusqueda = decodeURIComponent(textoBusqueda);
                    $("#GridSearchGeneral").val(textoBusqueda);
                    $("#GridSearchGeneral").focus();
                }

                if (flagContieneIdentificacion !== -1) {
                    var arrayParametros = parametro.split("="); // Separacion
                    var textoBusqueda = arrayParametros[1];
                    textoBusqueda = decodeURIComponent(textoBusqueda);
                    $("#GridSearchIdentificacion").val(textoBusqueda);
                    $("#GridSearchPlaca").val('');
                    $("#GridSearchIdentificacion").focus();
                }

                if (flagContienePlaca !== -1) {
                    var arrayParametros = parametro.split("="); // Separacion
                    var textoBusqueda = arrayParametros[1];
                    textoBusqueda = decodeURIComponent(textoBusqueda);
                    $("#GridSearchPlaca").val(textoBusqueda);
                    $("#GridSearchIdentificacion").val('');
                    $("#GridSearchPlaca").focus();
                }

            }
        });

        //Evento cuando existe alguna falla en la consulta al servidor
        document.addEventListener('reloadfail', function (e) {
            console.log('grid reloadfail: ', e.detail.grid);
            console.log('failed ajax response result: ', e.detail.result);
        });
    }

    //debugger  mvc-grid-value mvc-grid-apply mvc-grid-cancel
    ////Validar que los Grids tengan filtros de búsqueda
    //var filtrosBusqueda = document.getElementById('GridSearchGeneral');
    //if (filtrosBusqueda !== null) {
    //    document.getElementById('GridSearchGeneral').addEventListener('input', function () {
    //        var grid = new MvcGrid(document.querySelector('.mvc-grid'));
    //        grid.query.set('search', this.value);
    //        grid.reload();
    //    });
    //}

});


