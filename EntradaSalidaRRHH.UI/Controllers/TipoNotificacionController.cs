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
    public class TipoNotificacionController : BaseController
    {
        
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        // GET: TipoNotificaciones
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "TIEMPO ESPERA", "ESTADO" };
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridTipoNotificacion;
            //Búsqueda
            var listado = TipoNotificacionDAL.ListarTipoNotificaciones();
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
            return View();
        }

        [HttpPost]
        public ActionResult Create(TipoNotificacion tipoNotificacion)
        {
            try
            {
                string nombreTipoNotificacion = (tipoNotificacion.NombreNotificacion ?? string.Empty).ToLower().Trim();

                var tarifariosIguales = TipoNotificacionDAL.ListarTipoNotificaciones().Where(s => (s.NombreNotificacion ?? string.Empty).ToLower().Trim() == nombreTipoNotificacion).ToList();

                if (tarifariosIguales.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionNombreTarifario } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = TipoNotificacionDAL.CrearTipoNotificacion(tipoNotificacion);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tipoNotificacion = TipoNotificacionDAL.ConsultarNotificacion(id.Value);

            ViewBag.Tiempo = tipoNotificacion.TiempoEspera;

            if (tipoNotificacion == null)
            {
                return HttpNotFound();
            }
            return View(tipoNotificacion);
        }

        [HttpPost]
        public ActionResult Edit(TipoNotificacion tipoNotificacion)
        {
            try
            {
                string nombreTipoNotificacion = (tipoNotificacion.NombreNotificacion ?? string.Empty).ToLower().Trim();

                var tarifariosIguales = TipoNotificacionDAL.ListarTipoNotificaciones().Where(s => (s.NombreNotificacion ?? string.Empty).ToLower().Trim() == nombreTipoNotificacion && s.IdNotificacion != tipoNotificacion.IdNotificacion).ToList();

                if (tarifariosIguales.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionNombreTarifario } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = TipoNotificacionDAL.ActualizarTipoNotificacion(tipoNotificacion);

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
            RespuestaTransaccion resultado = TipoNotificacionDAL.EliminarTipoNotificacion(id);// await db.Cabecera.FindAsync(id);

            //return RedirectToAction("Index");
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = RolDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "TipoNotificacion.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = RolDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "TipoNotificacion.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = RolDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"TipoNotificacion.csv");
        }
        #endregion
    }
}