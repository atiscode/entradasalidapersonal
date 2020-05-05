using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.UI.Helper;
using EntradaSalidaRRHH.Repositorios;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog.Fluent;

namespace EntradaSalidaRRHH.UI.Controllers
{
    public class LoginController : BaseController
    {
        public ActionResult Index()
        {
            //Si el usuario ya tiene una sesión iniciada

            if (SessionHelper.GetDatosUsuariosSession().ResetClave == true)
            {
                return RedirectToAction("CambiarClave", "Login", new { id = SessionHelper.GetDatosUsuariosSession().IdUsuario });
            }
            else
            {
                if (SessionHelper.ValidarSesionUsuario())
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel formulario)
        {
            RespuestaTransaccion respuesta = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida, EntidadID = 0 };
            try
            {
                string usuario = formulario.Login;
                string password = formulario.Password;

                bool VerificacionCredenciales = UsuarioDAL.LoginCorrecto(usuario, password);

                Resultado.Estado = VerificacionCredenciales;

                if (VerificacionCredenciales)
                {
                    UsuarioLogeadoSession = UsuarioDAL.ConsultarLogin(usuario, password); // Almacenar Datos Usuario en sesión - global

                    if (!UsuarioLogeadoSession.EstadoUsuario.Value)
                    {
                        Resultado.Estado = false;
                        Resultado.Respuesta = Mensajes.MensajeUsuarioInactivo;
                    }
                    else
                    {
                        if (UsuarioLogeadoSession.ResetClave == false)
                        {
                            Resultado.Respuesta = Mensajes.MensajeUsuarioIngreso;
                        }
                        else
                        {
                            Resultado.Respuesta = Mensajes.MensajeCambioClave;
                        }
                    }
                }
                else
                {

                    Resultado.Respuesta = Mensajes.MensajeCredencialesIncorrectas;
                }
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult Register(RegisterViewModel formulario)
        {
            RespuestaTransaccion respuesta = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida, EntidadID = 0 };
            try
            {
                if (!Validaciones.ValidarMail(formulario.Email))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCorreoIncorrecto } }, JsonRequestBehavior.AllowGet);
                if (!Validaciones.VerificaIdentificacion(formulario.Identificacion))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeCedulaRucIncorrecto } }, JsonRequestBehavior.AllowGet);

                if (UsuarioDAL.NombreUsuarioExistente(formulario.UserName))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteNombreUsuario } }, JsonRequestBehavior.AllowGet);
                if (UsuarioDAL.IdentificacionExistente(formulario.Identificacion))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIdentificacion } }, JsonRequestBehavior.AllowGet);
                if (UsuarioDAL.CorreoExistente(formulario.Email))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistenteIMailCorporativo } }, JsonRequestBehavior.AllowGet);
                if (formulario.UserName.Length < 4)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudUserName } }, JsonRequestBehavior.AllowGet);
                if (formulario.Password.Length != formulario.ConfirmPassword.Length)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeValidacionContrasenias } }, JsonRequestBehavior.AllowGet);
                if (formulario.Password.Length < 8)
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeLongitudContrasenia } }, JsonRequestBehavior.AllowGet);


                Resultado = UsuarioDAL.CrearUsuarioGenerico(formulario);

                string enlace = GetUrlSitio(Url.Action("Index", "Login"));

                string body = GetEmailTemplate("TemplateRegistroUsuario");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Usuario", formulario.Nombre + " " + formulario.Apellidos);

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
                        CorreosDestinarios = formulario.Email,
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
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult RecoveryPassword(ForgotViewModel formulario)
        {
            try
            {
                Resultado = UsuarioDAL.RecuperarClave(formulario);
                string enlace = GetUrlSitio(Url.Action("Index", "Login"));

                string body = GetEmailTemplate("TemplateResetClaveUsuario");
                body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                body = body.Replace("@ViewBag.Usuario", formulario.Login);
                body = body.Replace("@ViewBag.Contrasenia", Resultado.EntidadID.ToString());


                if (Resultado.Estado)
                {
                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Reset Clave",
                        DescripcionTarea = "El usuario olvidó su contraseña.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = formulario.Login,
                        AsuntoCorreo = "RECUPERAR CLAVE",
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

        public ActionResult Logout()
        {
            SessionHelper.DestroyUserSession(); // Elimina la entidad UsuarioLogeado almacenada en Sesión
            Session.Clear(); // Elimina todas las variables en sesión
            return RedirectToAction("Index", "Login");
        }

        public ActionResult CambiarClave(int id)
        {
            CambiarClave cambiaClave = new CambiarClave()
            {
                UsuaCodi = id
            };
            return View(cambiaClave);
        }

        [HttpPost]
        public ActionResult CambiarClave(CambiarClave formulario)
        {
            RespuestaTransaccion respuesta = new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida, EntidadID = 0 };
            try
            {
                Resultado = UsuarioDAL.CambiarClaveReset(formulario);
                if (Resultado.Estado)
                {
                    SessionHelper.DestroyUserSession();
                    Session.Clear();

                    string enlace = GetUrlSitio(Url.Action("Index", "Login"));
                    var datosusuario = UsuarioDAL.ConsultarUsuario(Convert.ToInt32(formulario.UsuaCodi));
                    string body = GetEmailTemplate("TemplateCambioClaveUsuario");
                    body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                    body = body.Replace("@ViewBag.EnlaceSecundario", enlace);
                    body = body.Replace("@ViewBag.Usuario", datosusuario.NombresApellidos);

                    //Siempre que un usuario se haya creado con éxito.

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
    }
}