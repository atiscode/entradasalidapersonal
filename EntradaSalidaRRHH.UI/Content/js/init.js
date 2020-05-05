var _ALERT = {
    SUCCESS: 'alert-success',
    INFO: 'alert-info',
    WARNING: 'alert-warning',
    DANGER: 'alet-danger',
};


$(function () {


    $(".datepicker").datepicker({
        autoclose: true,
        format: 'dd/mm/yyyy',
    });
    //$('.js-example-basic-single').select2();
    //$('input[type="checkbox"], input[type="radio"]').iCheck({
    //    checkboxClass: "icheckbox_minimal-blue",
    //    radioClass: "iradio_minimal-blue"
    //});

    //$(".js-example-basic-multiple").select2({
    //    allowClear: true,
    //    /* Add this */
    //    placeholder: "Seleccionar ..",
    //    width: "100%",
    //    heigh: "100%",
    //});

    $(".editor-texto-html").wysihtml5({ locale: "es-ES" });
    $('.alert').alert()
    //Inicializando Tablas
    $('table.datatable-jquery').DataTable({
        destroy: true,
        responsive: true,
        scrollY: '200px',
        scroller: "true",
        paging: true,
        //fixedColumns: {
        //    leftColumns: 2
        //},
        "language": {
            "sProcessing": "<div style='margin-top: -58px;'><img style='width='100px'; height='100px''  src='/Content/imagenes2/loading.gif'/></div>",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados.",
            "sEmptyTable": "Ningún dato disponible en esta tabla.",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",
            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            },
            "oAria": {
                "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
                "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            }
        },
    })
    //    .on('draw.dt', function () {
    //    $("table.datatable-jquery").find("tbody").show();
    //});

    //$('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    //    $($.fn.dataTable.tables(true)).DataTable()
    //        .columns.adjust()
    //        .responsive.recalc();
    //});

    $("#datemask").inputmask("dd/mm/yyyy", { "placeholder": "dd/mm/yyyy" });
    $("#datemask2").inputmask("mm/dd/yyyy", { "placeholder": "mm/dd/yyyy" });
    $("[data-mask]").inputmask();

    $(".mask").inputmask('Regex', { regex: "^[0-9]{1,6}(\\.\\d{1,2})?$", "placeholder": "0" });
    $('.test').inputmask({ mask: "(99999999)|(99999999.9{1,4})", "placeholder": "0" })

    //$('.email').inputmask('Regex', { regex: "[a-zA-Z0-9._%-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,4}" });
    $('.email').inputmask("email");

    $('.test2').inputmask({ mask: "(99999999)|(99999999,9{1,4})", "placeholder": "0" })

    $(".campo-decimal-manual-1").inputmask('Regex', { regex: "^[0-9]{1,9}(\\.\\d{1,4})?$" });

    $(".campo-decimal-manual-2").inputmask('Regex', { regex: "^[0-9]{1,9}(\\,\\d{1,4})?$" });

    $(".campo-decimal").inputmask({
        'alias': 'decimal',
        'groupSeparator': '.',
        'autoGroup': true,
        'digits': 4,
        'digitsOptional': false,
        'placeholder': '0'
    });

    $(".campo-decimal-prefijo-dinero").inputmask({
        'alias': 'decimal',
        'groupSeparator': ',',
        'autoGroup': true,
        'digits': 2,
        'digitsOptional': false,
        'prefix': '$ ',
        'placeholder': '0'
    });
    // Fix sidebar white space at bottom of page on resize
    $(window).on("load", function () {
        setTimeout(function () {
            $("body").layout("fix");
            $("body").layout("fixSidebar");
        }, 250);
    });
});

//Integer Number
$(document).on("input", ".int-number", function (e) {
    this.value = this.value.replace(/[^0-9]/g, '');
});

$(".search-menu-box").on('input', function () {
    var filter = $(this).val().trim();
    $(".sidebar-menu > li").each(function () {
        debugger
        if (RemoveAccents($(this).text()).search(new RegExp(filter, "i")) < 0) {
            $(this).hide();
        } else {
            let elementos = $(this).find('ul');
            //$(this).slideToggle('fast');

            elementos = elementos.find('li');
            elementos.each(function (index, value) {
                debugger
                //console.log(value)
                let enlace = $(value).find('a');
                let texto = $(enlace).text();
                //if (Coincidence(texto, filter)) {
                if (texto.search(new RegExp(filter, "i")) > 0){
                    //console.log(texto)
                    $(value).addClass("subrayado-submenu");
                } else {
                    $(value).removeClass("subrayado-submenu");
                }
                //console.log($(enlace).text())
                //$(value).addClass("subrayado-submenu");
            })
            $(this).show();
        }
    });
});

function Coincidence(str1, str2) {
    var percent = similarity(str1, str2);
    return percent > 0.7 ? true : false;
}

function similarity(s1, s2) {
    var longer = s1;
    var shorter = s2;
    if (s1.length < s2.length) {
        longer = s2;
        shorter = s1;
    }
    var longerLength = longer.length;
    if (longerLength == 0) {
        return 1.0;
    }
    return (longerLength - editDistance(longer, shorter)) / parseFloat(longerLength);
}

function editDistance(s1, s2) {
    s1 = s1.toLowerCase();
    s2 = s2.toLowerCase();

    var costs = new Array();
    for (var i = 0; i <= s1.length; i++) {
        var lastValue = i;
        for (var j = 0; j <= s2.length; j++) {
            if (i == 0)
                costs[j] = j;
            else {
                if (j > 0) {
                    var newValue = costs[j - 1];
                    if (s1.charAt(i - 1) != s2.charAt(j - 1))
                        newValue = Math.min(Math.min(newValue, lastValue),
                            costs[j]) + 1;
                    costs[j - 1] = lastValue;
                    lastValue = newValue;
                }
            }
        }
        if (i > 0)
            costs[s2.length] = lastValue;
    }
    return costs[s2.length];
}

function RemoveAccents(strAccents) {
    var strAccents = strAccents.split('');
    var strAccentsOut = new Array();
    var strAccentsLen = strAccents.length;
    var accents = 'ÀÁÂÃÄÅàáâãäåÒÓÔÕÕÖØòóôõöøÈÉÊËèéêëðÇçÐÌÍÎÏìíîïÙÚÛÜùúûüÑñŠšŸÿýŽž';
    var accentsOut = "AAAAAAaaaaaaOOOOOOOooooooEEEEeeeeeCcDIIIIiiiiUUUUuuuuNnSsYyyZz";
    for (var y = 0; y < strAccentsLen; y++) {
        if (accents.indexOf(strAccents[y]) != -1) {
            strAccentsOut[y] = accentsOut.substr(accents.indexOf(strAccents[y]), 1);
        } else {
            strAccentsOut[y] = strAccents[y];
        }
    }
    strAccentsOut = strAccentsOut.join('');
    //console.log(strAccentsOut);
    return strAccentsOut;
}

function showAlertAditionals(alerttype, header, message, footer, timeOut = 5000) {
    debugger
    $('.content').prepend('<div id="alertdiv" role="alert" class="alert ' + alerttype + '"><button type="button" class="close" data-dismiss="alert" aria-label="Close"> <span aria-hidden="true">&times;</span> </button> <h4 class="alert-heading">' + header + '</h4> <span id="texto-alerta">' + message + '</span> <hr>  <small>'+ footer +'</small></div>')
    setTimeout(function () { // this will automatically close the alert and remove this if the users doesnt close it in 5 secs
        $("#alertdiv").remove();
    }, timeOut);
}