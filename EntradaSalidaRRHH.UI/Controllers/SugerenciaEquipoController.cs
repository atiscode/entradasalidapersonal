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
    public class SugerenciaEquipoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE EQUIPO", "TIPO", "CARACTERISTICAS", "CARGO" };

        // GET: SugerenciaEquipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<SugerenciaEquiposCargoInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridSugerenciaEquipos;
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
                        listado = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = SugerenciaEquipoDAL.ObtenerTotalRegistrosListadoSugerenciaEquipo();
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

        public ActionResult _Formulario(int? id)
        {
            ViewBag.TituloModal = Etiquetas.TituloPanelFormularioSugerenciaEquipos;
            try
            {
                SugerenciaEquiposCargo model = new SugerenciaEquiposCargo();
                List<string> programas = new List<string>();

                if (id.HasValue)
                {
                    model = SugerenciaEquipoDAL.ConsultarSugerenciaEquiposCargo(id.Value);
                    SugerenciaEquiposCargoInfo objeto = SugerenciaEquipoDAL.ListadoSugerenciaEquiposCargo(null, null, null, id).FirstOrDefault();

                    programas = objeto != null ? objeto.IDsProgramas.Split(',').ToList() : new List<string>();

                    var programasListado = CatalogoDAL.ObtenerListadoCatalogosByCodigoSeleccion("PROGRAMAS-01", null).Where(s => programas.Contains(s.Value)).Select(m => new SelectListItem
                    {
                        Text = m.Text,
                        Value = m.Value,
                    });

                    ViewBag.Programas = programasListado;
                }

                ViewBag.EquiposCompletos = EquipoDAL.ListadoEquipo();

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return PartialView(new SugerenciaEquiposCargo());
            }

        }

        [HttpPost]
        public ActionResult Create(SugerenciaEquiposCargo formulario, List<int> programas)
        {
            try
            {
                programas = programas ?? new List<int>();

                Resultado = SugerenciaEquipoDAL.CrearSugerenciaEquiposCargo(formulario, programas);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(SugerenciaEquiposCargo formulario, List<int> programas)
        {
            try
            {
                programas = programas ?? new List<int>();

                Resultado = SugerenciaEquipoDAL.ActualizarSugerenciaEquiposCargo(formulario, programas);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);

            RespuestaTransaccion resultado = SugerenciaEquipoDAL.EliminarSugerenciaEquiposCargo(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProgramas(string query)
        {
            var data = CatalogoDAL.ObtenerListadoCatalogosByCodigoSeleccion("PROGRAMAS-01", null).Select(m => new SelectListItem
            {
                Text = m.Text,
                Value = m.Value,
            })
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.StartsWith(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCargoEmpresa(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("CARGO", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = SugerenciaEquipoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = SugerenciaEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado Sugerencias de Equipo");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = SugerenciaEquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion

    }
}