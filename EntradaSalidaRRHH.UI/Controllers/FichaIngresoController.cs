using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
//Extensión para Query string dinámico
using System.Linq.Dynamic;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using System.IO;
using System.Configuration;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Drawing;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Converters;
using System.Web;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public partial class FichaIngresoOverload{
        public FichaIngreso formulario { get; set; }
    }
    public partial class UsuarioOverload
    {
        public Usuario usuario { get; set; }
    }
    public partial class ExperienciaOverload
    {
        public List<DetalleExperiencias> experiencias { get; set; }
    }
    public partial class CargasFamiliaresOverload
    {
        public List<DetalleCargasFamiliares> cargasFamiliares { get; set; }
    }
    public partial class EstudiosOverload
    {
        public List<DetalleEstudios> estudios { get; set; }
    }

    public class FichaIngresoController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRES Y APELLIDOS", "IDENTIFICACIÓN", "EMPRESA", "FECHA" };

        // GET: FichaIngreso
        [Autenticado]
        public ActionResult Index()
        {
            return View();
        }

        [Autenticado]
        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(string search, string sort = "", string order = "", long? page = 1)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            var listado = new List<FichasIngresosInfo>();
            ViewBag.NombreListado = Etiquetas.TituloGridFichaIngrso;
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
                        listado = FichaIngresoDAL.ListadoFichaIngreso(page.Value).OrderBy(sort + " " + order).ToList();
                    else
                        listado = FichaIngresoDAL.ListadoFichaIngreso(page.Value).ToList();
                }

                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

                if (!string.IsNullOrEmpty(search))//filter
                {
                    listado = FichaIngresoDAL.ListadoFichaIngreso(null, search);
                }

                if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(order))
                        listado = FichaIngresoDAL.ListadoFichaIngreso(null, null, whereClause).OrderBy(sort + " " + order).ToList();
                    else
                        listado = FichaIngresoDAL.ListadoFichaIngreso(null, null, whereClause);
                }
                else
                {

                    if (string.IsNullOrEmpty(search))
                        totalPaginas = FichaIngresoDAL.ObtenerTotalRegistrosListadoFichaIngreso();
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

        [Autenticado]
        public ActionResult Formulario(int? id, int? usuarioID, bool readOnly = false)
        {

            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioFichaIngreso;
            ViewBag.UsuarioID = usuarioID ?? 0;
            ViewBag.interno = true; // Para saber que Layout va a utilizar el usuario.
            ViewBag.Readonly = readOnly;

            try
            {
                FichaIngresoDetallada model = new FichaIngresoDetallada();
                model.FichaIngreso.FechaIngresoFicha = DateTime.Now;

                if (id.HasValue)
                {
                    model = FichaIngresoDAL.ConsultarFichaIngresoDetallada(id.Value);
                    ViewBag.UsuarioID = model.FichaIngreso.UsuarioID;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View(new FichaIngreso());
            }
        }

        //http://localhost:62098/FichaIngreso/Formulario?usuarioID=220 --> URL PARA USUARIOS EXTERNOS
        public ActionResult NuevoIngreso(int? id, int? usuarioID, bool interno = false)
        {
            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioFichaIngreso;
            ViewBag.UsuarioID = usuarioID ?? 0;
            ViewBag.interno = interno; // Para saber que Layout va a utilizar el usuario.
            try
            {
                FichaIngresoDetallada model = new FichaIngresoDetallada();
                model.FichaIngreso.FechaIngresoFicha = DateTime.Now;

                bool fichaIngresoExistente = FichaIngresoDAL.FichaIngresoExistente(usuarioID.Value);

                if (fichaIngresoExistente)
                {
                    return RedirectToAction("Index");
                }

                if (id.HasValue)
                {
                    model = FichaIngresoDAL.ConsultarFichaIngresoDetallada(id.Value);
                    ViewBag.UsuarioID = model.FichaIngreso.UsuarioID;
                }
                return View("Formulario", model);
            }
            catch (Exception ex)
            {
                return View("Formulario", new FichaIngreso());
            }
        }

        [Autenticado]
        public ActionResult _SeleccionarUsuario()
        {
            ViewBag.TituloModal = "Seleccionar el Usuario.";
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create(FichaIngreso formulario, List<DetalleEstudios> estudios, List<DetalleExperiencias> experiencias, List<DetalleCargasFamiliares> cargasFamiliares, Usuario usuario)
        {
            string FileName = string.Empty;
            try
            {
                estudios = estudios ?? new List<DetalleEstudios>();
                experiencias = experiencias ?? new List<DetalleExperiencias>();
                cargasFamiliares = cargasFamiliares ?? new List<DetalleCargasFamiliares>();

                var fichaIngreso = HttpContext.Request.Params.Get("formulario");
                formulario = JsonConvert.DeserializeObject<FichaIngresoOverload>(fichaIngreso, settingsJsonDeserilize).formulario;

                var datosUsuario = HttpContext.Request.Params.Get("usuario");
                usuario = JsonConvert.DeserializeObject<UsuarioOverload>(datosUsuario).usuario;

                var dataEstudios = HttpContext.Request.Params.Get("estudios");
                estudios = JsonConvert.DeserializeObject<EstudiosOverload>(dataEstudios).estudios;

                var dataExperiencias = HttpContext.Request.Params.Get("experiencias");
                experiencias = JsonConvert.DeserializeObject<ExperienciaOverload>(dataExperiencias, settingsJsonDeserilize).experiencias;

                var dataCargasFamiliares = HttpContext.Request.Params.Get("cargasFamiliares");
                cargasFamiliares = JsonConvert.DeserializeObject<CargasFamiliaresOverload>(dataCargasFamiliares, settingsJsonDeserilize).cargasFamiliares;

                //Usuario
                usuario.IdUsuario = formulario.UsuarioID;

                //Obtener archivos
                HttpFileCollectionBase files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    string path = string.Empty;

                    // Checking for Internet Explorer    
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        path = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        path = file.FileName;
                        FileName = file.FileName;
                    }

                    string tipoArchivo = file.ContentType;

                    using (var reader = new BinaryReader(file.InputStream))
                    {
                        formulario.Foto = reader.ReadBytes(file.ContentLength);
                    }
                }

                usuario.Direccion = formulario.DireccionCallePrincipal + " ; " + formulario.DireccionCalleSecundaria + " ; " + formulario.DireccionConjuntoResidencial + " N. " + formulario.DireccionNumeroCasa;

                var actualizacionDatosUsuario = UsuarioDAL.ActualizarDatosUsuarioFichaIngreso(usuario);

                if (actualizacionDatosUsuario.Estado)
                {
                    var destinatarios = PerfilesDAL.ConsultarCorreoNotificacion(14);
                    string enlace = GetUrlSitio(Url.Action("Index", "FichaIngreso"));

                    string body = GetEmailTemplate("TemplateFormularioIngreso");
                    body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                    body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                    body = body.Replace("@ViewBag.Empleado", usuario.Nombres + " " + usuario.Apellidos);

                    Resultado = FichaIngresoDAL.CrearFichaIngreso(formulario, cargasFamiliares, estudios, experiencias);
                    //Siempre que la ficha de ingreso haya sido creado con éxito.
                    if (Resultado.Estado)
                    {
                        var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                        {
                            NombreTarea = "Nueva Ficha Ingreso",
                            DescripcionTarea = "Correo de notificación de registro de ficha un nuevo empleado en el corporativo a RRHH y al área médica.",
                            NombreEmisor = nombreCorreoEmisor,
                            CorreoEmisor = correoEmisor,
                            ClaveCorreo = claveEmisor,
                            CorreosDestinarios = destinatarios,
                            AsuntoCorreo = "NOTIFICACION DE FICHA DE INGRESO",
                            NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                            CuerpoCorreo = body,
                            AdjuntosCorreo = "",//ruta,
                            FechaEnvioCorreo = DateTime.Now,
                            Empresa = "ATISCODE",
                            Canal = CanalNotificaciones,
                            Tipo = "FICHA INGRESO",
                        });
                    }
                }
                else
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = actualizacionDatosUsuario.Respuesta } }, JsonRequestBehavior.AllowGet);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Excepción al crear ficha de ingreso.");
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [Autenticado]
        [HttpPost]
        public ActionResult Edit(FichaIngreso formulario, List<DetalleEstudios> estudios, List<DetalleExperiencias> experiencias, List<DetalleCargasFamiliares> cargasFamiliares, Usuario usuario)
        {
            string FileName = string.Empty;
            try
            {
                estudios = estudios ?? new List<DetalleEstudios>();
                experiencias = experiencias ?? new List<DetalleExperiencias>();
                cargasFamiliares = cargasFamiliares ?? new List<DetalleCargasFamiliares>();

                estudios = estudios ?? new List<DetalleEstudios>();
                experiencias = experiencias ?? new List<DetalleExperiencias>();
                cargasFamiliares = cargasFamiliares ?? new List<DetalleCargasFamiliares>();

                var fichaIngreso = HttpContext.Request.Params.Get("formulario");
                formulario = JsonConvert.DeserializeObject<FichaIngresoOverload>(fichaIngreso, settingsJsonDeserilize).formulario;

                var datosUsuario = HttpContext.Request.Params.Get("usuario");
                usuario = JsonConvert.DeserializeObject<UsuarioOverload>(datosUsuario).usuario;

                var dataEstudios = HttpContext.Request.Params.Get("estudios");
                estudios = JsonConvert.DeserializeObject<EstudiosOverload>(dataEstudios).estudios;

                var dataExperiencias = HttpContext.Request.Params.Get("experiencias");
                experiencias = JsonConvert.DeserializeObject<ExperienciaOverload>(dataExperiencias, settingsJsonDeserilize).experiencias;

                var dataCargasFamiliares = HttpContext.Request.Params.Get("cargasFamiliares");
                cargasFamiliares = JsonConvert.DeserializeObject<CargasFamiliaresOverload>(dataCargasFamiliares, settingsJsonDeserilize).cargasFamiliares;

                //Usuario
                usuario.IdUsuario = formulario.UsuarioID;

                //Obtener archivos
                HttpFileCollectionBase files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    string path = string.Empty;

                    // Checking for Internet Explorer    
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        path = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        path = file.FileName;
                        FileName = file.FileName;
                    }

                    string tipoArchivo = file.ContentType;

                    using (var reader = new BinaryReader(file.InputStream))
                    {
                        formulario.Foto = reader.ReadBytes(file.ContentLength);
                    }
                }

                usuario.Direccion = formulario.DireccionCallePrincipal + " ; " + formulario.DireccionCalleSecundaria + " ; " + formulario.DireccionConjuntoResidencial + " N. " + formulario.DireccionNumeroCasa;

                var actualizacionDatosUsuario = UsuarioDAL.ActualizarDatosUsuarioFichaIngreso(usuario);

                if (actualizacionDatosUsuario.Estado)
                    Resultado = FichaIngresoDAL.ActualizarFichaIngreso(formulario, cargasFamiliares, estudios, experiencias);
                else
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = actualizacionDatosUsuario.Respuesta } }, JsonRequestBehavior.AllowGet);

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Excepción al editar ficha de ingreso.");
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [Autenticado]
        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);

            RespuestaTransaccion resultado = FichaIngresoDAL.EliminarFichaIngreso(id);
            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);
        }

        //JSON para el combo provincia
        public JsonResult GetCiudad(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("CIUDAD", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUsuariosFichasIngreso(string query)
        {
            query = !string.IsNullOrEmpty(query) ? query.ToLower().Trim() : string.Empty;

            var data = FichaIngresoDAL.ObtenerListadoUsuariosFichasIngreso()
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.ToLower().Contains(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region REPORTES BASICOS
        [Autenticado]
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = FichaIngresoDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "Listado.xlsx");
        }

        [Autenticado]
        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = FichaIngresoDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList(), "Listado Fichas de Ingreso");

            return File(buffer, PDFContentType, "ReportePDF.pdf");
        }

        [Autenticado]
        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = FichaIngresoDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"Listado.csv");
        }
        #endregion

    }
}