using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using OfficeOpenXml;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    // GET: Rol
    public partial class RolPerfiles
    {
        public RolPerfiles()
        {
            IdsPerfilesGuardar = new List<int>();
            IdsPerfilesEditar = new List<int>();
        }
        public int IdRol { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Nullable<bool> Estado { get; set; }
        public List<int> IdsPerfilesGuardar { get; set; }
        public List<int> IdsPerfilesEditar { get; set; }

    }

    [Autenticado]
    public class RolController : BaseController
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "ESTADO" };
        // GET: Rol
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridRol;
            //Búsqueda
            var listado = RolDAL.ListarRol();

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

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var obj = RolDAL.ConsultarRol(id.Value);

            if (obj == null)
                return HttpNotFound();
            else
                return View(obj);
        }

        public ActionResult Create()
        {
            SelectList Listado = new SelectList(PerfilesDAL.ListadoPerfil(), "Value", "Text");
            ViewBag.idsPerfiles = Listado;

            return View();
        }

        [HttpPost]
        public ActionResult Create(RolPerfiles rol, List<int> perfiles)
        {
            try
            {

                string nombreRol = (rol.Nombre ?? string.Empty).ToLower().Trim();

                var validacionNombreRolUnico = RolDAL.ListarRol().Where(s => (s.Nombre ?? string.Empty).ToLower().Trim() == nombreRol).ToList();

                if (validacionNombreRolUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionNombreRol } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = RolDAL.CrearRol(new Rol { Nombre = rol.Nombre, Descripcion = rol.Descripcion }, perfiles);


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
            var rol = RolDAL.ConsultarRol(id.Value);

            ViewBag.idsPerfilesRol = RolDAL.ListadoRolPerfil(id.Value);
            SelectList perfiles = new SelectList(PerfilesDAL.ListadoPerfil(), "Value", "Text");
            ViewBag.idsPerfiles = perfiles;

            if (rol == null)
            {
                return HttpNotFound();
            }
            return View(rol);
        }

        [HttpPost]
        public ActionResult Edit(RolPerfiles rol, List<int> perfiles)
        {
            try
            {

                string nombreRol = (rol.Nombre ?? string.Empty).ToLower().Trim();

                var validacionNombreRolUnico = RolDAL.ListarRol().Where(s => (s.Nombre ?? string.Empty).ToLower().Trim() == nombreRol && s.IdRol != rol.IdRol).ToList();

                if (validacionNombreRolUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionNombreRol } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = RolDAL.ActualizarRol(new Rol { IdRol = rol.IdRol, Nombre = rol.Nombre, Descripcion = rol.Descripcion, Estado = rol.Estado }, perfiles);

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
            RespuestaTransaccion resultado = RolDAL.EliminarRol(id);// await db.Cabecera.FindAsync(id);

            //return RedirectToAction("Index");
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

        }



        public JsonResult _GetPerfiles()
        {
            List<MultiSelectJQuery> items = new List<MultiSelectJQuery>();

            items = PerfilesDAL.ListarPerfil()
.Select(o => new MultiSelectJQuery(o.IdPerfil, o.NombrePerfil, o.DescripcionPerfil)).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = RolDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoRoles.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = RolDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoRoles.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = RolDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoRoles.csv");
        }
        #endregion


    }


}
