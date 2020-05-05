using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

//Extensión para Query string dinámico
using System.Linq.Dynamic;

using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using EntradaSalidaRRHH.UI.Helper;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class AsignacionEquipoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "FECHA DE SOLICITUD", "USUARIO", "EQUIPOS", "HERRAMIENTAS ADICIONALES", "EMPRESA", "CARGO", "DEPARTAMENTO" };


        // GET: AsignacionEquipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<AsignacionEquipoInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridAsignacionEquipo;
            page = page > 0 ? page - 1 : page;
            int totalPaginas = 1;
            try
            {
                var query = (HttpContext.Request.Params.Get("QUERY_STRING") ?? "").ToString();

                var dynamicQueryString = GetQueryString(query);
                var whereClause = BuildWhereDynamicClause(dynamicQueryString);

                //Siempre y cuando no haya filtros definidos en el Grid
                if (string.IsNullOrEmpty(whereClause))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = AsignacionEquipoDAL.ListadoAsignacionEquipo(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = AsignacionEquipoDAL.ListadoAsignacionEquipo(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = AsignacionEquipoDAL.ListadoAsignacionEquipo(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = AsignacionEquipoDAL.ListadoAsignacionEquipo(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = AsignacionEquipoDAL.ListadoAsignacionEquipo(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = AsignacionEquipoDAL.ObtenerTotalRegistrosListadoAsignacionEquipo();
                }

                ViewBag.TotalPaginas = totalPaginas;

                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
            catch (Exception ex)
            {
                ViewBag.TotalPaginas = totalPaginas;
                // Only grid query values will be available here.
                return PartialView(await Task.Run(() => listado));
            }
        }

        public ActionResult GestionarAsignacion(int? id)
        {

            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioRequerimientoEquipos;
            ViewBag.UsuarioID = 0;

            try
            {
                RequerimientoEquipo model = new RequerimientoEquipo();
                List<string> equipos = new List<string>();
                List<string> herramientasAdicionales = new List<string>();

                if (id.HasValue)
                {
                    model = RequerimientoEquipoDAL.ConsultarRequerimientoEquipo(id.Value);

                    RequerimientoEquipoInfo objeto = RequerimientoEquipoDAL.ListadoRequerimientoEquipo(null, null, null, id).FirstOrDefault();

                    equipos = objeto != null ? objeto.IDsEquipos.Split(',').ToList() : new List<string>();

                    var equiposListado = EquipoDAL.ObtenerListadoEquipos().Where(s => equipos.Contains(s.Value)).Select(m => new SelectListItem
                    {
                        Text = m.Text,
                        Value = m.Value,
                    });

                    ViewBag.Equipos = equiposListado;

                    herramientasAdicionales = objeto != null ? objeto.IDsHerramientasAdicionales.Split(',').ToList() : new List<string>();
                    var herramientasAdicionalesListado = EquipoDAL.ObtenerListadoEquipos(null, " WHERE CodigoCatalogoTipo = 'ACCESORIOS-01' ").Where(s => herramientasAdicionales.Contains(s.Value) && !string.IsNullOrEmpty(s.Value)).Select(m => new SelectListItem
                    {
                        Text = m.Text,
                        Value = m.Value,
                    });

                    ViewBag.HerramientasAdicionales = herramientasAdicionalesListado;

                    ViewBag.UsuarioID = model.UsuarioID;
                }

                ViewBag.Asignado = true;

                return View("~/Views/RequerimientoEquipo/Formulario.cshtml", model);
            }
            catch (Exception ex)
            {
                return View("~/Views/RequerimientoEquipo/Formulario.cshtml",new FichaIngreso());
            }
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = AsignacionEquipoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = AsignacionEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado de Asignaciones");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = AsignacionEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion

    }
}