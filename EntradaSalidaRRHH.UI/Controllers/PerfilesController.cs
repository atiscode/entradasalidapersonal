using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using System;
using System.Collections.Generic;
using EntradaSalidaRRHH.UI.Helper;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using OfficeOpenXml;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public partial class PerfilesOpcionesMenu
    {
        public PerfilesOpcionesMenu()
        {
            IdsOpcionesMenuGuardar = new List<int>();
            IdsOpcionesMenuEditar = new List<int>();
        }
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }
        public string DescripcionPerfil { get; set; }
        public Nullable<bool> EstadoPerfil { get; set; }
        public List<int> IdsOpcionesMenuGuardar { get; set; }
        public List<int> IdsOpcionesMenuEditar { get; set; }

    }

    [Autenticado]

    public class PerfilesController : BaseController
    {
        // GET: Perfiles
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE", "DESCRIPCIÓN", "ESTADO" };
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridPerfil;
            //Búsqueda
            var listado = PerfilesDAL.ListarPerfil();
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

        // GET: Perfiles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var obj = PerfilesDAL.ConsultarPerfil(id.Value);

            if (obj == null)
                return HttpNotFound();
            else
                return View(obj);
        }

        // GET: Perfiles/Create
        public ActionResult Create()
        {
            SelectList ListadoMenu = new SelectList(MenuDAL.ListadoMeruHijos(), "Value", "Text");
            ViewBag.listadoMenu = ListadoMenu;
            return View();
        }

        [HttpPost]
        public ActionResult Create(PerfilesOpcionesMenu perfil, List<int> opcionesMenu)
        {
            try
            {
                string nombrePerfil = (perfil.NombrePerfil ?? string.Empty).ToLower().Trim();

                var validacionNombreUnico = PerfilesDAL.ListarPerfil().Where(s => (s.NombrePerfil ?? string.Empty).ToLower().Trim() == nombrePerfil).ToList();

                if (validacionNombreUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente } }, JsonRequestBehavior.AllowGet);



                RespuestaTransaccion resultado = PerfilesDAL.CrearPerfil(new Perfil { NombrePerfil = perfil.NombrePerfil, DescripcionPerfil = perfil.DescripcionPerfil }, opcionesMenu);

                return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Perfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var rol = PerfilesDAL.ConsultarPerfil(id.Value);
            SelectList ListadoMenu = new SelectList(MenuDAL.ListadoMeruHijos(), "Value", "Text");
            ViewBag.listadoMenu = ListadoMenu;

            var opcionesMenu = PerfilesDAL.ListadoPerfilMenu(id.Value);
            ViewBag.idsPerfilesOpcionesMenu = opcionesMenu;

            if (rol == null)
            {
                return HttpNotFound();
            }
            return View(rol);
        }

        [HttpPost]
        public ActionResult Edit(PerfilesOpcionesMenu perfil, List<int> opcionesMenu)
        {
            try
            {

                string nombrePerfil = (perfil.NombrePerfil ?? string.Empty).ToLower().Trim();

                var validacionNombreUnico = PerfilesDAL.ListarPerfil().Where(s => (s.NombrePerfil ?? string.Empty).ToLower().Trim() == nombrePerfil && s.IdPerfil != perfil.IdPerfil).ToList();

                if (validacionNombreUnico.Count > 0)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente } }, JsonRequestBehavior.AllowGet);

                RespuestaTransaccion resultado = PerfilesDAL.ActualizarPerfil(new Perfil { IdPerfil = perfil.IdPerfil, EstadoPerfil = perfil.EstadoPerfil, NombrePerfil = perfil.NombrePerfil, DescripcionPerfil = perfil.DescripcionPerfil }, opcionesMenu);

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
            RespuestaTransaccion resultado = PerfilesDAL.EliminarPerfil(id);

            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

      
        public JsonResult _GetOpcionesMenu()
        {
            List<MultiSelectJQuery> items = new List<MultiSelectJQuery>();

            items = MenuDAL.ListarMenuHijos()
.Select(o => new MultiSelectJQuery(o.IdMenu, o.NombreMenu, o.NombrePaginaMenu)).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }


        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = PerfilesDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoPerfiles.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = PerfilesDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoPerfiles.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = PerfilesDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoPerfiles.csv");
        }
        #endregion

    }


}