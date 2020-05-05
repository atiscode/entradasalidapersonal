using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class ParametrosController : BaseController
    {
        // GET: Parametros
     
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "VALOR", "TIPO", "ESTADO" };

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridParametros;
            //Búsqueda
            var listado = ParametrosDAL.ListarParametros();
            search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

            if (!string.IsNullOrEmpty(search))//filter
            {
                var type = listado.GetType().GetGenericArguments()[0];
                var properties = type.GetProperties();

                listado = listado.Where(x => properties
                            .Any(p =>
                            {
                                var value = p.GetValue(x);
                                return value != null && value.ToString().ToLower().Contains(search.ToLower());
                            })).ToList();
            }

            // Only grid query values will be available here.
            return PartialView(await Task.Run(() => listado));
        }

        
        public ActionResult Create()
        {
            var tipodato = CatalogoDAL.ListadoCatalogosPorCodigo("TDATO-01");
            ViewBag.ListadoTipo = tipodato;
            return View();
        }

        [HttpPost]
        public ActionResult Create(ParametrosSistema parametros)
        {
            try
            {
                string nombrePlan = (parametros.Nombre ?? string.Empty).ToLower().Trim();

                RespuestaTransaccion resultado = ParametrosDAL.CrearParametrosSistema(parametros);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Tarifarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var parametro = ParametrosDAL.ConsultarParametros(id.Value);

            if (parametro == null)
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.Valor = parametro.Valor.ToString();
                var tipodato = CatalogoDAL.ListadoCatalogosPorCodigo("TDATO-01");
                ViewBag.tipodato = tipodato;

                return View(parametro);
            }
        }

        [HttpPost]
        public ActionResult Edit(ParametrosSistema parametros)
        {
            try
            {
                string nombrePlan = (parametros.Nombre ?? string.Empty).ToLower().Trim();

                RespuestaTransaccion resultado = ParametrosDAL.ActualizarParametrosSistema(parametros);
                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
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
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RespuestaTransaccion resultado = ParametrosDAL.EliminarParametrosSistema(id);// await db.Cabecera.FindAsync(id);

            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }


        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = ParametrosDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoParametros.xlsx");
        }


        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = ParametrosDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoParametros.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = ParametrosDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoParametros.csv");
        }
        #endregion
    }
}