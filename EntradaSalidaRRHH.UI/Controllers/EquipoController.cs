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
    public class EquipoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "TIPO", "CARACTERÍSTICAS", "OBSERVACIONES" , "COSTO", "PROVEEDOR" };

        // GET: Equipo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<EquiposInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridEquipo;
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
                        listado = EquipoDAL.ListadoEquipo(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = EquipoDAL.ListadoEquipo(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = EquipoDAL.ListadoEquipo(null, search);//CotizacionEntity.ListadoGestionPrefacturaSAFI();//
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = EquipoDAL.ListadoEquipo(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = EquipoDAL.ListadoEquipo(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = EquipoDAL.ObtenerTotalRegistrosListadoEquipo();
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
            ViewBag.TituloModal = Etiquetas.TituloPanelFormularioEquipo;
            try
            {
                Equipo model = new Equipo();
                List<string> caracteristicasEquipo = new List<string>();

                if (id.HasValue)
                {
                    model = EquipoDAL.ConsultarEquipo(id.Value);
                    EquiposInfo objeto = EquipoDAL.ListadoEquipo(null, null, null, id).FirstOrDefault();

                    caracteristicasEquipo = objeto != null ? objeto.IDsCaracteristicas.Split(',').ToList() : new List<string>();

                    var caracteristicas = CatalogoDAL.ObtenerListadoCatalogosByCodigoSeleccion("CARACTERISTICAS-EQUIPOS-01", null).Where(s=> caracteristicasEquipo.Contains(s.Value)).Select(m => new SelectListItem
                    {
                        Text = m.Text,
                        Value = m.Value,
                    });

                    ViewBag.Caracteristicas = caracteristicas;
                }

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return PartialView(new Equipo());
            }

        }

        [HttpPost]
        public ActionResult Create(Equipo formulario, List<int> caracteristicas)
        {
            try
            {
                Resultado = EquipoDAL.CrearEquipo(formulario, caracteristicas);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(Equipo formulario ,List<int> caracteristicas)
        {
            try
            {
                Resultado = EquipoDAL.ActualizarEquipo(formulario, caracteristicas);

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

            RespuestaTransaccion resultado = EquipoDAL.EliminarEquipo(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCaracteristicas(string query)
        {
            var data = CatalogoDAL.ObtenerListadoCatalogosByCodigoSeleccion("CARACTERISTICAS-EQUIPOS-01", null).Select(m => new SelectListItem
            {
                Text = m.Text,
                Value = m.Value,
            })
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.StartsWith(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = EquipoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = EquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado de Equipos");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = EquipoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion

    }
}