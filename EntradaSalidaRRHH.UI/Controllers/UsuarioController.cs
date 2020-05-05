using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using NLog;
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
    [Autenticado]
    public class UsuarioController : BaseController
    {
        private List<string> columnasReportesBasicos = new List<string> { "NOMBRE COMPLETOS", "IDENTIFICACIÓN", "NOMBRE USUARIO", "DEPARTAMENTO", "ÁREA", "CARGO", "PAÍS", "CIUDAD", "DIRECCIÓN", "MAIL", "MAIL CORPORATIVO", "TELÉFONO", "CELULAR", "ROL", "ESTADO" };
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> _IndexGrid(String search)
        {
            //Permisos
            Permisos(ControllerContext.RouteData.Values["controller"].ToString());

            ViewBag.NombreListado = Etiquetas.TituloGridUsuario;
            //Búsqueda
            var listado = UsuarioDAL.ListarUsuarios();

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

        public ActionResult CambiarClave()
        {

            var id = UsuarioLogeadoSession.IdUsuario;
            ViewBag.idUsuario = id;
            DAL.Modelo.CambiarClave cambiaClave = new CambiarClave();
            return View();
        }

        [HttpPost]
        public ActionResult CambiarClave(CambiarClave usuario)
        {
            try
            {

                Resultado = UsuarioDAL.CambiarClave(usuario);

                string enlace = GetUrlSitio(Url.Action("Index", "Login"));
                var datosusuario = UsuarioDAL.ConsultarUsuario(Convert.ToInt32(usuario.idUsuario));
                string body = GetEmailTemplate("TemplateCambioClaveUsuario");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Usuario", datosusuario.NombresApellidos);

                //Siempre que un usuario se haya creado con éxito.
                if (Resultado.Estado)
                {
                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Cambio de Usuario",
                        DescripcionTarea = "El usuario cambio de clave.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = datosusuario.MailCorporativo,
                        AsuntoCorreo = "CAMBIO DE CLAVE",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = "",
                        Canal = CanalNotificaciones,
                        Tipo = "USUARIO",
                    });
                }

                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                Resultado.Respuesta = Resultado.Respuesta + ";" + ex.Message;
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Usuario usuario)
        {
            RespuestaTransaccion resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida };
            try
            {
                if (!Validaciones.ValidarMail(usuario.MailCorporativo))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoCorporativoIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (!Validaciones.ValidarMail(usuario.Mail))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (!Validaciones.VerificaIdentificacion(usuario.Identificacion))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCedulaRucIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (usuario.Username.Length < 4)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudUserName } }, JsonRequestBehavior.AllowGet);

                Resultado = UsuarioDAL.CrearUsuario(usuario);
                string enlace = GetUrlSitio(Url.Action("Index", "Login"));

                string body = GetEmailTemplate("TemplateBienvenidaUsuario");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Usuario", usuario.Nombres + " " + usuario.Apellidos);
                body = body.Replace("@ViewBag.Contrasenia", Cryptography.Decrypt(usuario.Clave));

                //Siempre que un usuario se haya creado con éxito.
                if (Resultado.Estado)
                {
                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Creación de Usuario",
                        DescripcionTarea = "Registro de un nuevo usuario.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = usuario.MailCorporativo,
                        AsuntoCorreo = "CREACION DE USUARIO",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = "",//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        Empresa = "",
                        Canal = CanalNotificaciones,
                        Tipo = "USUARIO",
                    });
                }
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                Resultado.Respuesta = Resultado.Respuesta + ";" + ex.Message;
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var usuario = UsuarioDAL.ConsultarUsuarioEdit(id.Value);

                if (usuario == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(usuario);
                }
            }

        }

        [HttpPost]
        public ActionResult Edit(Usuario usuario)
        {

            try
            {
                if (!Validaciones.ValidarMail(usuario.Mail))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (!Validaciones.VerificaIdentificacion(usuario.Identificacion))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCedulaRucIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (usuario.Username.Length < 4)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudUserName } }, JsonRequestBehavior.AllowGet);

                Resultado = UsuarioDAL.ActualizarUsuario(usuario);
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Resultado.Respuesta = Resultado.Respuesta + ";" + ex.Message;
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RespuestaTransaccion resultado = UsuarioDAL.EliminarUsuario(id);

            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult CambioEstado(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RespuestaTransaccion resultado = UsuarioDAL.DesactivarUsuario(id);

            return Json(new { Resultado = resultado }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCargo(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("CARGO", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJefe(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("JEFE", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        //JSON para el combo provincia
        public JsonResult GetCiudad(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("CIUDAD", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmpresaUsuario(int id)
        {
            var ficha = FichaIngresoDAL.ConsultarFichaIngreso(id);
            var usuario = UsuarioDAL.ConsultarInformacionPrincipalUsuario(ficha.UsuarioID);
            int empresa = usuario != null ? usuario.IdEmpresa.Value : 0;
            return Json(empresa, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetArea(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("ÁREA", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartamento(int id)
        {
            var datos = CatalogoDAL.ListadoCatalogosPorCodigoId("DEPARTAMENTO", id);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditUsuario()
        {

            var id = UsuarioLogeadoSession.IdUsuario;


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var usuario = UsuarioDAL.ConsultarUsuarioEdit(id);

                if (usuario == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(usuario);
                }
            }

        }

        [HttpPost]
        public ActionResult EditUsuario(Usuario usuario)
        {
            try
            {
                Resultado = UsuarioDAL.ActualizarUsuario(usuario);
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Resultado.Respuesta = Resultado.Respuesta + ";" + ex.Message;
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Bienvenida()
        {
            return View();
        }

        #region REPORTES BASICOS
        public ActionResult DescargarReporteFormatoExcel()
        {
            var collection = UsuarioDAL.ListadoReporteBasico();
            var package = GetEXCEL(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(package.GetAsByteArray(), XlsxContentType, "ListadoUsuarios.xlsx");
        }

        public ActionResult DescargarReporteFormatoPDF()
        {
            // Seleccionar las columnas a exportar
            var collection = UsuarioDAL.ListadoReporteBasico();
            byte[] buffer = GetPDF(columnasReportesBasicos, collection.Cast<object>().ToList());

            return File(buffer, PDFContentType, "ListadoUsuarios.pdf");
        }

        public ActionResult DescargarReporteFormatoCSV()
        {
            var collection = UsuarioDAL.ListadoReporteBasico();
            byte[] buffer = GetCSV(columnasReportesBasicos, collection.Cast<object>().ToList());
            return File(buffer, CSVContentType, $"ListadoUsuarios.csv");
        }
        #endregion
    }
}