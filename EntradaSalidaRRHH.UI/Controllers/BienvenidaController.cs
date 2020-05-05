using EntradaSalidaRRHH.DAL.Metodos;
using EntradaSalidaRRHH.DAL.Modelo;
using EntradaSalidaRRHH.Repositorios;
using EntradaSalidaRRHH.UI.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntradaSalidaRRHH.UI.Controllers
{
    [Autenticado]
    public class BienvenidaController : BaseController
    {
        // GET: Bienvenida
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _Formulario(int? id)
        {
            var model = id.HasValue ? UsuarioDAL.ConsultarUsuario(id.Value) : new UsuarioInfo();

            ViewBag.TituloModal = "BIENVENIDA";
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Create(Usuario formulario)
        {
            try
            {
                //#region Búsqueda de archivos adjuntos en directorio
                //Catalogo catalogo = CatalogoDAL.ConsultarCatalogo(formulario.IdEmpresa.Value);

                ////El nombre del archivo de acumulación de décimos tiene que ser igual al código del catálogo de la empresa seleccionada.
                //string nombreArchivo = catalogo.CodigoCatalogo;

                //if (string.IsNullOrEmpty(nombreArchivo))
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeErrorParametrizacionArchivoEmpresaDirectorio, catalogo) } }, JsonRequestBehavior.AllowGet);

                ////SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                //string rutaBase = basePathRepositorioDocumentos + "\\RRHH\\Documentos\\AcumulacionDecimos";

                //nombreArchivo += ".docx";
                //string pathServidor = Path.Combine(rutaBase, nombreArchivo);

                //if (!Directory.Exists(pathServidor))
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeErrorArchivoNoEncontrado, nombreArchivo) } }, JsonRequestBehavior.AllowGet);

                //string rutaBaseDocumentosIngreso = basePathRepositorioDocumentos + "\\RRHH\\Documentos\\Otros";
                //string nombreArchivoDocumentoIngreso = "Formulario Documentos de Ingreso.xlsx";
                //string pathDocumentosIngreso = Path.Combine(rutaBaseDocumentosIngreso, nombreArchivoDocumentoIngreso);

                //if (!Directory.Exists(pathDocumentosIngreso))
                //    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = string.Format(Mensajes.MensajeErrorArchivoNoEncontrado, nombreArchivoDocumentoIngreso) } }, JsonRequestBehavior.AllowGet);
                //#endregion

                Catalogo catalogo = CatalogoDAL.ConsultarCatalogo(formulario.IdEmpresa.Value);

                //El nombre del archivo de acumulación de décimos tiene que ser igual al código del catálogo de la empresa seleccionada.
                string nombreArchivo = catalogo.CodigoCatalogo;

                if (string.IsNullOrEmpty(nombreArchivo))
                    return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = "El código de catálogo es requerido para la empresa " + catalogo } }, JsonRequestBehavior.AllowGet);

                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];

                //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                string rutaBase = basePath + "\\RRHH\\Documentos\\AcumulacionDecimos";

                nombreArchivo += ".docx";
                string pathServidor = Path.Combine(rutaBase, nombreArchivo);

                //SI LA RUTA EN DISCO NO EXISTE LOS ARCHIVOS SE ALMACENAN EN LA CARPETA MISMO DEL PROYECTO
                string ruta = AppDomain.CurrentDomain.BaseDirectory + "Documentos/AcumulacionDecimos/" + nombreArchivo;

                // En caso de que no exista el directorio, crearlo.
                bool directorio = Directory.Exists(pathServidor);

                string rutaBaseDocumentosIngreso = AppDomain.CurrentDomain.BaseDirectory + "Documentos/Otros/";
                string pathDocumentosIngreso = Path.Combine(rutaBaseDocumentosIngreso, "Formulario Documentos de Ingreso.xlsx");

                bool existeUsuario = UsuarioDAL.VerificarCorreoUsuarioExistente(formulario.Mail);

                //Solo registra el usuario si no existe
                if (!existeUsuario)
                    Resultado = UsuarioDAL.CrearUsuario(formulario);
                else
                {
                    //Si el usuario ya existe, setear el ID, para que lo tome en el envio de la notificacion
                    var usuario = UsuarioDAL.ConsultaUsuarioByEmail(formulario.Mail);
                    Resultado.EntidadID = usuario.IdUsuario;

                    Resultado.Estado = true;
                    Resultado.Respuesta = Mensajes.MensajeBienvenidaUsuarioExistente;
                }

                //Siempre que el usuario haya sido creado con éxito.
                if (Resultado.Estado || existeUsuario)
                {
                    string enlace = GetUrlSitio(Url.Action("NuevoIngreso", "FichaIngreso", new { usuarioID = Resultado.EntidadID }));//Url.Action("NuevoIngreso", "FichaIngreso", new { usuarioID = Resultado.EntidadID }, Request.Url.Scheme);

                    string body = GetEmailTemplate("TemplateBienvenida");
                    body = body.Replace("@ViewBag.EnlaceDirecto", enlace);
                    body = body.Replace("@ViewBag.EnlaceSecundario", enlace);

                    //Adjuntar tambien el archivo de Documentos Ingreso
                    if (!string.IsNullOrEmpty(pathServidor))
                        pathServidor += ";" + pathDocumentosIngreso;

                    var notificacion = NotificacionesDAL.CrearNotificacion(new Notificaciones
                    {
                        NombreTarea = "Bienvenida",
                        DescripcionTarea = "Correo de bienvenida a los nuevos usuarios que ingresan a formar parte del corporativo.",
                        NombreEmisor = nombreCorreoEmisor,
                        CorreoEmisor = correoEmisor,
                        ClaveCorreo = claveEmisor,
                        CorreosDestinarios = formulario.Mail,
                        AsuntoCorreo = "BIENVENIDA",
                        NombreArchivoPlantillaCorreo = TemplateNotificaciones,
                        CuerpoCorreo = body,
                        AdjuntosCorreo = pathServidor,//ruta,
                        FechaEnvioCorreo = DateTime.Now,
                        //DetalleEstadoEjecucionNotificacion = string.Empty,
                        Empresa = catalogo.NombreCatalogo,
                        Canal = CanalNotificaciones,
                        Tipo = "BIENVENIDA",
                    });
                }

                return Json(new { Resultado = Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Resultado = new RespuestaTransaccion { Estado = false, Respuesta = ex.Message } }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}