//using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Helper
{
    public partial class MenuParcial
    {
        public int MenuID { get; set; }
        public int AccionID { get; set; }
        public string NombreAccionCatalogo { get; set; }
        public string Controlador { get; set; }
        public string Metodo { get; set; }
    }
    public partial class UsuarioRolMenuPermisoR
    {
        public int IDRolMenuPermiso { get; set; }
        public int RolID { get; set; }
        public string NombreRol { get; set; }
        public int PerfilID { get; set; }
        public string NombrePerfil { get; set; }
        public int MenuID { get; set; }
        public string NombreMenu { get; set; }
        public string EnlaceMenu { get; set; }
        public string MenuPadre { get; set; }
        public int IDCatalogo { get; set; }
        public string CodigoCatalogo { get; set; }
        public string TextoCatalogoAccion { get; set; }
        public int? CreadoPorID { get; set; }
        public string CreadoPor { get; set; }
        public int? ActualizadoPorID { get; set; }
        public string ActualizadoPor { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Estado { get; set; }
        public string MetodoControlador { get; set; }
        public string NombreControlador { get; set; }
        public string AccionEnlace { get; set; }
    }

    public static class ControlPermisos
    {
        //private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        public static string ConvertToListHtml(string prefacturas, string ids, string enlace)
        {
            string resultado = string.Empty;
            try
            {

                if (!string.IsNullOrEmpty(prefacturas) && !string.IsNullOrEmpty(ids))
                {
                    var itemsIDS = ids.Split(',');//listado.Split(',');
                    var listadoPrefacturas = prefacturas.Split(',');

                    int indice = 0;
                    foreach (string idPrefactura in itemsIDS)
                    {
                        string prefactura = listadoPrefacturas.ElementAt(indice);
                        string enlaceDescarga = enlace + "?listadoIDs=" + idPrefactura + "&descargaDirecta=" + true;
                        resultado += "<ul><li><a title='Descargar prefactura.' href='" + enlaceDescarga + "'> #" + prefactura + "</a></li></ul>";
                        //resultado += "<li> <a href='" + enlace + "'" + @service + "</a></li>";
                        indice++;
                    }

                    //<a href="index.html">valid</a>
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return resultado;
            }
        }


        public static string ConvertToLinkDescargaHtml(string id, string nombreArchivo, string enlace)
        {
            string resultado = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(nombreArchivo) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(enlace))
                {
                    string enlaceDescarga = enlace + "?id=" + id;
                    resultado += "<ul><li><a title='Descargar Archivo.' href='" + enlaceDescarga + "'>" + nombreArchivo + "</a></li></ul>";
                }
                return resultado;
            }
            catch (Exception ex)
            {
                return resultado;
            }
        }
        
        public static IHtmlString GetBotones(List<UsuarioRolMenuPermisoR> listado, List<string> accionesControladorSeleccionado = null, string nombreControlador = null, string gridID = null)
        {
            var cadena = "";
            try
            {
                if (accionesControladorSeleccionado != null)
                {
                    listado = listado.Where(s => accionesControladorSeleccionado.Contains(s.MetodoControlador)).ToList();



                    string functionalidadbusqueda = "<div class='input-group'><span class='input-group-addon'><i class='glyphicon glyphicon-search'></i></span> " +
                        " <input type = 'text' maxlength='50' name='name' id='GridSearchGeneral' placeholder='Búsqueda General' class='busqueda-general'>  </div> ";

                    string textoToolTipBotonAgregar = "Agregar nuevo registro.";
                    string clsIconoAgregar = "fa fa-plus";
                    string colorBotonAgregar = "#f59324";
                    string functionalidadAgregarRegistro = "<button onclick='CrearRegistro();' title='" + textoToolTipBotonAgregar + "' style='background-color:" + colorBotonAgregar + "; border-color: " + colorBotonAgregar + ";' class='btn btn-primary' id='nuevo' ;><i class='" + clsIconoAgregar + "' aria-hidden='true'></i></button>";

                    string textoToolTipBotonReporte = "Descargar información.";
                    string clsIconoReporte = "fa fa-download";
                    string colorBotonReporte = "#808080";
                    string functionalidadDescargaReportes = "<button  title='" + textoToolTipBotonReporte + "' style='background-color:" + colorBotonReporte + "; border-color: " + colorBotonReporte + ";' data-toggle='modal' data-target='#export-modal' class='btn btn-outline-info'  id='reporte'><i style='color: black;' class='" + clsIconoReporte + "' aria-hidden='true'></i></button>";

                    string textoToolTipBotonRecargarGrid = "Recargar información.";
                    string clsIconoRecargarGrid = "glyphicon glyphicon-refresh";
                    string colorBotonRecargarGrid = "#003C59";
                    string functionalidadRecargarGrid = "<button onclick='RecargarDatosGrid();' title ='" + textoToolTipBotonRecargarGrid + "' styl class='btn btn-success' id='recargar'><i class='" + clsIconoRecargarGrid + "' aria-hidden='true'></i></button>";



                    if (nombreControlador != "ManejoPermisos")
                    {
                        foreach (var item in listado)
                        {
                            switch (item.CodigoCatalogo)
                            {
                                case "ACCIONES-SIST-BUSQUEDA":

                                    cadena += functionalidadbusqueda + " ";
                                    break;
                                case "ACCIONES-SIST-CREAR":
                                    if (gridID != "grid-Subcatalogo")
                                    {
                                        cadena += functionalidadAgregarRegistro + " ";
                                    }
                                    else
                                    {
                                        cadena += " " + " ";
                                    }
                                    break;

                                case "ACCIONES-SIST-REPORTES-BASICOS":
                                    if (gridID != "grid-Subcatalogo")
                                    {

                                        cadena += functionalidadDescargaReportes + " ";
                                    }
                                    else
                                    {
                                        cadena += " " + " ";
                                    }
                                    break;
                                case "ACCIONES-SIST-RECARGAR":
                                    cadena += functionalidadRecargarGrid + " ";
                                    
                                    break;


                                default:
                                    cadena += String.Empty;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        cadena = functionalidadbusqueda + " " + functionalidadDescargaReportes + " " + functionalidadRecargarGrid;
                    }
                }

                return new HtmlString(cadena);
            }
            catch (Exception)
            {

                return new HtmlString(cadena);
            }

        }
        public static string GestionBontonesGrid(List<string> listado, string comando)
        {
            string clase = "ocultar-accion-catalogo";
            try
            {
                clase = listado.Any(s => s == comando) ? "mostrar-accion-catalogo" : "ocultar-accion-catalogo";
                return clase;
            }
            catch (Exception ex)
            {
                return clase;
            }
        }

        public static string VerificarPermisoAccionControlador(DataRow fila, int indiceColumna, List<SelectListItem> ListadoMenusAplicacion = null, List<SelectListItem> ListadoAccionesAplicacion = null)
        {
            string clase = "Error-Metodo-VerificarPermisoAccionControlador";
            try
            {
                string columnName = fila.Table.Columns[indiceColumna].ColumnName;

                switch (columnName)
                {
                    case "N":
                    case "NOMBREMENU":
                    case "MenuID":
                    case "PerfilID":
                    case "RolID":
                        clase = "Centrar";
                        break;
                    default:
                        bool valorColumna = Convert.ToBoolean(fila.ItemArray[indiceColumna]); // Valor del Check

                        var access = ListadoAccionesAplicacion.Where(s => columnName == s.Text.Split('|')[0]).FirstOrDefault();
                        var method = string.Empty;

                        if (access != null)
                            method = access.Text.Split('|')[1];

                        int menuID = int.Parse((fila["MenuID"] ?? "0").ToString());

                        var menu = ListadoMenusAplicacion.FirstOrDefault(s => s.Value == menuID.ToString());
                        string nombreControlador = menu != null ? menu.Text.Split('/')[0] : string.Empty;

                        var acciones = ActionNames(nombreControlador);

                        bool ok = acciones.Any(s => s == method);

                        if (!ok)
                            clase = "Centrar bloquear-accion-catalogo";
                        else
                            clase = "Centrar";
                        break;
                }
                return clase;
            }
            catch (Exception ex)
            {
                return clase;
            }
        }

        public static IHtmlString GetCheckBoxTitle(string titulo, string idElemento, string claseElemento)
        {
            IHtmlString final = new HtmlString("");
            try
            {
                string elemento = "<input class='" + claseElemento + "' id='" + idElemento + "' name='CheckAll' title='Seleccionar todos los elementos mostrados en la página.' type='checkbox' value='false'>" + "<label>" + titulo + "</label>";
                final = new HtmlString(elemento);
                return final;
            }
            catch (Exception ex)
            {
                return final;
            }
        }

        public static bool AccionPermitidaControlador(DataRow fila, int indiceColumna, List<SelectListItem> ListadoMenusAplicacion = null, List<SelectListItem> ListadoAccionesAplicacion = null)
        {
            bool permitido = false;
            try
            {
                string columnName = fila.Table.Columns[indiceColumna].ColumnName;

                switch (columnName)
                {
                    case "N":
                    case "NOMBREMENU":
                    case "MenuID":
                    case "PerfilID":
                    case "RolID":
                        permitido = true;
                        break;
                    default:
                        bool valorColumna = Convert.ToBoolean(fila.ItemArray[indiceColumna]); // Valor del Check

                        var access = ListadoAccionesAplicacion.Where(s => columnName == s.Text.Split('|')[0]).FirstOrDefault();
                        var method = string.Empty;

                        if (access != null)
                            method = access.Text.Split('|')[1];

                        int menuID = int.Parse((fila["MenuID"] ?? "0").ToString());

                        var menu = ListadoMenusAplicacion.FirstOrDefault(s => s.Value == menuID.ToString());
                        string nombreControlador = menu != null ? menu.Text.Split('/')[0] : string.Empty;

                        var acciones = ActionNames(nombreControlador);

                        bool ok = acciones.Any(s => s == method);

                        if (!ok)
                            permitido = false;
                        else
                            permitido = true;

                        break;
                }
                return permitido;
            }
            catch (Exception ex)
            {
                return permitido;
            }
        }

        public static string SetFilaTemporal(DataRow fila)
        {
            HttpContext.Current.Session["numeroColumna"] = fila;
            return "fila-almacenada";
        }

        public static DataRow GetFilaTemporal()
        {
            DataTable table = new DataTable();
            DataRow row = table.NewRow();
            try
            {
                row = HttpContext.Current.Session["Fila"] != null ? (DataRow)HttpContext.Current.Session["Fila"] : null;
                return row;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> ActionNames(string controllerName)
        {
            var types =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where typeof(IController).IsAssignableFrom(t) &&
                    string.Equals(controllerName + "Controller", t.Name, StringComparison.OrdinalIgnoreCase)
                select t;

            var controllerType = types.FirstOrDefault();

            if (controllerType == null)
            {
                return Enumerable.Empty<string>().ToList();
            }
            return new ReflectedControllerDescriptor(controllerType)
               .GetCanonicalActions().Select(x => x.ActionName).ToList();
        }

    }
}